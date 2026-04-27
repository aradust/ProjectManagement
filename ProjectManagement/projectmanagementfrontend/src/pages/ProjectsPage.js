import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { fetchProjects, getProjectDocuments, deleteProject, downloadDocument } from "../api/api.js";
import {
    Box,
    Typography,
    Card,
    CardContent,
    TextField,
    InputAdornment,
    CircularProgress,
    Button,
    Chip,
    Stack,
    List,
    ListItem,
    ListItemText,
    Collapse,
    IconButton,
    Tooltip,
} from "@mui/material";
import SearchIcon from "@mui/icons-material/Search";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";
import AttachFileIcon from "@mui/icons-material/AttachFile";
import ExpandMoreIcon from "@mui/icons-material/ExpandMore";
import ExpandLessIcon from "@mui/icons-material/ExpandLess";
import VisibilityIcon from "@mui/icons-material/Visibility";
import { DatePicker } from "@mui/x-date-pickers/DatePicker";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import dayjs from "dayjs";
import Layout from "../components/Layout";
import { useAuth } from "../contexts/AuthContext";

export default function ProjectPage() {
    const navigate = useNavigate();
    const { user, hasRole } = useAuth();

    const [projects, setProjects] = useState([]);
    const [isLoading, setIsLoading] = useState(false);
    const [documentsMap, setDocumentsMap] = useState({});
    const [expandedDocs, setExpandedDocs] = useState({});

    const [searchQuery, setSearchQuery] = useState("");
    const [startDate, setStartDate] = useState(null);
    const [endDate, setEndDate] = useState(null);
    const [priorityFrom, setPriorityFrom] = useState("");
    const [priorityTo, setPriorityTo] = useState("");

    const [priorityFromError, setPriorityFromError] = useState("");
    const [priorityToError, setPriorityToError] = useState("");

    const today = dayjs().startOf("day");

    useEffect(() => {
        loadProjects();
    }, [searchQuery, startDate, endDate, priorityFrom, priorityTo]);

    const validatePriority = (value) => {
        if (value === "") return "";
        if (!/^\d+$/.test(value)) return "Only digits allowed";
        const num = Number(value);
        if (!Number.isInteger(num)) return "Must be integer";
        if (num < 1) return "Must be at least 1";
        if (num > 10) return "Max 10";
        return "";
    };

    const handlePriorityChange = (value, setter, errorSetter) => {
        setter(value);
        errorSetter(validatePriority(value));
    };

    const loadProjects = async () => {
        setIsLoading(true);
        try {
            const filters = {};

            if (searchQuery?.trim()) {
                filters.search = searchQuery.trim();
            }

            if (startDate) {
                filters.startDateFrom = startDate.format("YYYY-MM-DD");
            }

            if (endDate) {
                filters.startDateTo = endDate.format("YYYY-MM-DD");
            }

            if (priorityFrom && !priorityFromError) {
                filters.priorityFrom = Number(priorityFrom);
            }

            if (priorityTo && !priorityToError) {
                filters.priorityTo = Number(priorityTo);
            }

            const data = await fetchProjects(filters);
            setProjects(data);

            const initialDocsMap = {};
            data.forEach((p) => {
                initialDocsMap[p.id] = { docs: [], loading: false };
            });
            setDocumentsMap(initialDocsMap);
        } catch (error) {
            console.error("Failed to load projects:", error);
        } finally {
            setIsLoading(false);
        }
    };

    const loadDocumentsForProject = async (projectId) => {
        if (documentsMap[projectId]?.loading) return;

        setDocumentsMap((prev) => ({
            ...prev,
            [projectId]: { ...prev[projectId], loading: true },
        }));

        try {
            const docs = await getProjectDocuments(projectId);
            setDocumentsMap((prev) => ({
                ...prev,
                [projectId]: { docs, loading: false },
            }));
        } catch {
            setDocumentsMap((prev) => ({
                ...prev,
                [projectId]: { ...prev[projectId], loading: false },
            }));
        }
    };

    const toggleDocuments = (projectId) => {
        const isCurrentlyExpanded = expandedDocs[projectId];
        if (!isCurrentlyExpanded) {
            if (!documentsMap[projectId]?.docs.length && !documentsMap[projectId]?.loading) {
                loadDocumentsForProject(projectId);
            }
        }
        setExpandedDocs((prev) => ({
            ...prev,
            [projectId]: !isCurrentlyExpanded,
        }));
    };

    const canEditProject = (project) => {
        if (hasRole("Chief")) return true;
        if (hasRole("Manager")) return project.projectManagerId === user?.id;
        return false;
    };

    const canDeleteProject = (project) => {
        return canEditProject(project);
    };

    const handleEditClick = (project) => {
        navigate("/create", {
            state: {
                mode: "edit",
                projectId: project.id,
            },
        });
    };

    const handleDeleteClick = async (project) => {
        if (!confirm(`Delete project "${project.projectName}"? This action cannot be undone.`)) return;
        try {
            await deleteProject(project.id);
            setProjects((prev) => prev.filter((p) => p.id !== project.id));
            setDocumentsMap((prev) => {
                const newMap = { ...prev };
                delete newMap[project.id];
                return newMap;
            });
            setExpandedDocs((prev) => {
                const newExpanded = { ...prev };
                delete newExpanded[project.id];
                return newExpanded;
            });
        } catch (e) {
            alert("Failed to delete project: " + e.message);
        }
    };

    const handleViewDocument = async (doc) => {
        try {
            const blob = await downloadDocument(doc.id);
            const url = window.URL.createObjectURL(blob);
            window.open(url, "_blank");
            setTimeout(() => window.URL.revokeObjectURL(url), 1000);
        } catch (error) {
            console.error("Failed to download document:", error);
            alert("Could not open document");
        }
    };

    const formatFileSize = (bytes) => {
        if (bytes < 1024) return `${bytes} B`;
        if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
        return `${(bytes / (1024 * 1024)).toFixed(2)} MB`;
    };

    const formatDisplayDate = (dateString) => {
        if (!dateString) return "";
        return dayjs(dateString).format("DD.MM.YYYY");
    };

    return (
        <LocalizationProvider dateAdapter={AdapterDayjs}>
            <Layout title="Projects">
                <Box sx={{ maxWidth: 1200, mx: "auto" }}>
                    <Box sx={{ mb: 4, display: "flex", flexDirection: "column", gap: 2 }}>
                        <TextField
                            fullWidth
                            label="Search projects by name or company"
                            variant="outlined"
                            value={searchQuery}
                            onChange={(e) => setSearchQuery(e.target.value)}
                            slotProps={{
                                input: {
                                    startAdornment: (
                                        <InputAdornment position="start">
                                            <SearchIcon />
                                        </InputAdornment>
                                    ),
                                },
                            }}
                        />

                        <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>
                            <DatePicker
                                label="Start date from"
                                value={startDate}
                                onChange={setStartDate}
                                maxDate={endDate || undefined}
                                slotProps={{
                                    textField: {
                                        fullWidth: true,
                                        size: "small",
                                        placeholder: "DD.MM.YYYY"
                                    },
                                }}
                            />
                            <DatePicker
                                label="Start date to"
                                value={endDate}
                                onChange={setEndDate}
                                minDate={startDate || today}
                                slotProps={{
                                    textField: {
                                        fullWidth: true,
                                        size: "small",
                                        placeholder: "DD.MM.YYYY"
                                    },
                                }}
                            />
                        </Stack>

                        <Stack direction={{ xs: "column", sm: "row" }} spacing={2}>
                            <TextField
                                label="Priority from (1-10)"
                                value={priorityFrom}
                                onChange={(e) => {
                                    const val = e.target.value;
                                    if (/^\d*$/.test(val)) {
                                        handlePriorityChange(val, setPriorityFrom, setPriorityFromError);
                                    }
                                }}
                                error={!!priorityFromError}
                                helperText={priorityFromError}
                                size="small"
                                fullWidth
                                placeholder="1"
                            />
                            <TextField
                                label="Priority to (1-10)"
                                value={priorityTo}
                                onChange={(e) => {
                                    const val = e.target.value;
                                    if (/^\d*$/.test(val)) {
                                        handlePriorityChange(val, setPriorityTo, setPriorityToError);
                                    }
                                }}
                                error={!!priorityToError}
                                helperText={priorityToError}
                                size="small"
                                fullWidth
                                placeholder="10"
                            />
                        </Stack>

                        {(startDate || endDate || priorityFrom || priorityTo || searchQuery) && (
                            <Button
                                variant="outlined"
                                size="small"
                                onClick={() => {
                                    setSearchQuery("");
                                    setStartDate(null);
                                    setEndDate(null);
                                    setPriorityFrom("");
                                    setPriorityTo("");
                                    setPriorityFromError("");
                                    setPriorityToError("");
                                }}
                                sx={{ alignSelf: "flex-start" }}
                            >
                                Clear all filters
                            </Button>
                        )}
                    </Box>

                    {isLoading ? (
                        <Box sx={{ display: "flex", justifyContent: "center", p: 4 }}>
                            <CircularProgress />
                        </Box>
                    ) : (
                        <Box sx={{ display: "flex", flexWrap: "wrap", gap: 3 }}>
                            {projects.length === 0 && (
                                <Box sx={{ width: "100%", textAlign: "center", py: 4 }}>
                                    <Typography align="center" color="text.secondary">
                                        No projects found
                                    </Typography>
                                </Box>
                            )}

                            {projects.map((project) => {
                                const docState = documentsMap[project.id] || { docs: [], loading: false };
                                const docsCount = docState.docs.length;
                                const isExpanded = expandedDocs[project.id] || false;

                                return (
                                    <Box key={project.id} sx={{ width: { xs: "100%", md: "calc(50% - 12px)" } }}>
                                        <Card sx={{ height: "100%", display: "flex", flexDirection: "column" }}>
                                            <CardContent sx={{ flexGrow: 1, pb: 1 }}>
                                                <Box
                                                    sx={{
                                                        display: "flex",
                                                        justifyContent: "space-between",
                                                        alignItems: "flex-start",
                                                        mb: 2,
                                                    }}
                                                >
                                                    <Typography variant="h5" component="div">
                                                        {project.projectName}
                                                    </Typography>
                                                    <Chip
                                                        label={`Priority: ${project.priority}`}
                                                        color={project.priority > 5 ? "error" : "success"}
                                                        size="small"
                                                    />
                                                </Box>
                                                <Typography variant="body2" color="text.secondary" sx={{ mb: 0.5 }}>
                                                    Client: {project.customerCompanyName}
                                                </Typography>
                                                <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                                                    Executor: {project.executorCompanyName}
                                                </Typography>
                                                <Typography variant="caption" color="text.secondary">
                                                    📅 {formatDisplayDate(project.projectStart)} — {formatDisplayDate(project.projectEnd)}
                                                </Typography>

                                                <Box sx={{ mt: 2 }}>
                                                    <Stack direction="row" sx={{ alignItems: "center", spacing: 1 }}>
                                                        <IconButton
                                                            size="small"
                                                            onClick={() => toggleDocuments(project.id)}
                                                        >
                                                            {isExpanded ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                                                        </IconButton>
                                                        <Typography variant="body2" color="text.secondary">
                                                            Documents ({docsCount})
                                                        </Typography>
                                                        {docState.loading && <CircularProgress size={16} />}
                                                    </Stack>

                                                    <Collapse in={isExpanded}>
                                                        {docsCount > 0 ? (
                                                            <List dense disablePadding sx={{ mt: 1 }}>
                                                                {docState.docs.map((doc) => (
                                                                    <ListItem
                                                                        key={doc.id}
                                                                        disableGutters
                                                                        secondaryAction={
                                                                            <Tooltip title="View document">
                                                                                <IconButton size="small" onClick={() => handleViewDocument(doc)}>
                                                                                    <VisibilityIcon fontSize="small" />
                                                                                </IconButton>
                                                                            </Tooltip>
                                                                        }
                                                                    >
                                                                        <AttachFileIcon fontSize="small" sx={{ mr: 1, color: "text.secondary" }} />
                                                                        <ListItemText
                                                                            primary={doc.originalName}
                                                                            secondary={formatFileSize(doc.size)}
                                                                        />
                                                                    </ListItem>
                                                                ))}
                                                            </List>
                                                        ) : (
                                                            !docState.loading && (
                                                                <Typography variant="caption" color="text.secondary" sx={{ mt: 1, display: "block" }}>
                                                                    No documents attached
                                                                </Typography>
                                                            )
                                                        )}
                                                    </Collapse>
                                                </Box>
                                            </CardContent>
                                            <Box sx={{ p: 2, pt: 0, display: "flex", justifyContent: "flex-end", gap: 1 }}>
                                                {canDeleteProject(project) && (
                                                    <Tooltip title="Delete project">
                                                        <IconButton color="error" onClick={() => handleDeleteClick(project)}>
                                                            <DeleteIcon />
                                                        </IconButton>
                                                    </Tooltip>
                                                )}
                                                {canEditProject(project) && (
                                                    <Button
                                                        size="small"
                                                        variant="contained"
                                                        startIcon={<EditIcon />}
                                                        onClick={() => handleEditClick(project)}
                                                    >
                                                        Edit
                                                    </Button>
                                                )}
                                            </Box>
                                        </Card>
                                    </Box>
                                );
                            })}
                        </Box>
                    )}
                </Box>
            </Layout>
        </LocalizationProvider>
    );
}