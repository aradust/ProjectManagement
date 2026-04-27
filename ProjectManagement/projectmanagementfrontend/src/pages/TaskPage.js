import { useState, useEffect, useCallback } from "react";
import {
    Box, Button, Stack, Typography, Card, CardContent, CardActions,
    TextField, CircularProgress, Paper, MenuItem, Select, InputLabel, FormControl,
    Chip, IconButton, Autocomplete, Tooltip, Divider, Fade, FormHelperText
} from "@mui/material";
import {
    Delete as DeleteIcon,
    Edit as EditIcon,
    Add as AddIcon,
    CheckCircle,
    RadioButtonUnchecked,
    Autorenew,
    Person as PersonOutline,
    FlagOutlined,
} from "@mui/icons-material";
import { fetchTasks, createTask, updateTask, deleteTask, checkTaskTitleExists } from "../api/api.js";
import { fetchProjects, fetchEmployees } from "../api/api.js";
import Layout from "../components/Layout";
import { useAuth } from "../contexts/AuthContext";

const priorityColors = {
    1: "info",
    2: "info",
    3: "info",
    4: "info",
    5: "info",
    6: "warning",
    7: "warning",
    8: "warning",
    9: "error",
    10: "error"
};

const STATUS_MAP = {
    0: { label: "ToDo", color: "default", icon: <RadioButtonUnchecked fontSize="small" /> },
    1: { label: "In Progress", color: "info", icon: <Autorenew fontSize="small" /> },
    2: { label: "Done", color: "success", icon: <CheckCircle fontSize="small" /> }
};

const statusOptions = [
    { value: "", label: "All Statuses" },
    { value: "0", label: "ToDo" },
    { value: "1", label: "In Progress" },
    { value: "2", label: "Done" }
];

const sortOptions = [
    { value: "", label: "None" },
    { value: "priority", label: "Priority" },
    { value: "status", label: "Status" }
];

export default function TaskPage() {
    const { hasRole } = useAuth();
    const isChief = hasRole("Chief");
    const isManager = hasRole("Manager") && !isChief;
    const isEmployee = hasRole("Employee") && !isChief && !isManager;

    const canCreateTask = isChief;
    const canDeleteTask = isChief;
    const canChangeStatus = () => isEmployee || isManager || isChief;
    const canChangeAssignee = () => isManager || isChief;

    const [projects, setProjects] = useState([]);
    const [employees, setEmployees] = useState([]);
    const [tasks, setTasks] = useState([]);
    const [isLoading, setIsLoading] = useState(false);

    const [filterProject, setFilterProject] = useState(null);
    const [filterStatus, setFilterStatus] = useState(statusOptions[0]);
    const [sortBy, setSortBy] = useState(sortOptions[0]);
    const [searchTerm, setSearchTerm] = useState("");

    const [form, setForm] = useState({
        id: null, title: "", comment: "", priority: 1, status: 0,
        projectId: "", authorId: "", executorId: ""
    });
    const [errors, setErrors] = useState({});
    const [isEditing, setIsEditing] = useState(false);
    const [isCheckingTitle, setIsCheckingTitle] = useState(false);
    const [showCreateForm, setShowCreateForm] = useState(false);

    useEffect(() => {
        const loadRefs = async () => {
            const [projs, emps] = await Promise.all([fetchProjects(), fetchEmployees()]);
            setProjects(projs);
            setEmployees(emps);
        };
        loadRefs();
    }, []);

    useEffect(() => {
        loadTasks();
    }, [filterProject, filterStatus, sortBy]);

    const loadTasks = async () => {
        setIsLoading(true);
        try {
            const data = await fetchTasks({
                projectId: filterProject?.id || undefined,
                status: filterStatus?.value !== "" ? filterStatus?.value : undefined,
                sort: sortBy?.value || undefined
            });
            setTasks(data);
        } finally {
            setIsLoading(false);
        }
    };

    const validateField = (name, value) => {
        switch (name) {
            case "title":
                if (!value) return "Required";
                if (value.length < 3 || value.length > 100) return "Length must be 3-100 characters";
                return "";
            case "priority": {
                const num = Number(value);
                if (isNaN(num)) return "Enter a number";
                if (!Number.isInteger(num)) return "Must be an integer";
                if (num < 1 || num > 10) return "Priority must be 1-10";
                return "";
            }
            case "projectId": return value ? "" : "Select a project";
            case "authorId": return value ? "" : "Select an author";
            default: return "";
        }
    };

    const checkTitleUnique = useCallback(async (title) => {
        if (!title || !form.projectId) return false;
        if (validateField("title", title) !== "") return false;

        setIsCheckingTitle(true);
        try {
            const exists = await checkTaskTitleExists(title, form.projectId, form.id);
            if (exists) {
                setErrors(prev => ({ ...prev, title: "Task with this title already exists in the selected project" }));
            } else {
                setErrors(prev => ({ ...prev, title: "" }));
            }
            return exists;
        } finally {
            setIsCheckingTitle(false);
        }
    }, [form.projectId, form.id]);

    const handleFieldChange = (field, value) => {
        setForm(prev => ({ ...prev, [field]: value }));
        const error = validateField(field, value);
        setErrors(prev => ({ ...prev, [field]: error }));

        if (field === "projectId") {
            setErrors(prev => ({ ...prev, title: validateField("title", form.title) }));
            if (form.title && validateField("title", form.title) === "") {
                checkTitleUnique(form.title);
            }
        }
    };

    const isFormValid = () => {
        const requiredFields = ["title", "priority", "projectId", "authorId"];
        return requiredFields.every(field => validateField(field, form[field]) === "")
            && !errors.title && !isCheckingTitle;
    };

    const handleSave = async () => {
        if (isCheckingTitle) return;
        if (errors.title) return;

        const exists = await checkTitleUnique(form.title);
        if (exists) return;

        const newErrors = {
            title: validateField("title", form.title),
            priority: validateField("priority", form.priority),
            projectId: validateField("projectId", form.projectId),
            authorId: validateField("authorId", form.authorId)
        };
        setErrors(newErrors);
        if (Object.values(newErrors).some(e => e !== "")) return;

        try {
            if (isEditing) {
                await updateTask(form);
            } else {
                await createTask(form);
            }
            resetForm();
            loadTasks();
        } catch {
            alert("Error saving task");
        }
    };

    const handleDelete = async (id) => {
        if (!confirm("Delete task?")) return;
        try {
            await deleteTask(id);
            loadTasks();
            if (isEditing && form.id === id) resetForm();
        } catch {
            alert("Error deleting task");
        }
    };

    const handleStatusChange = async (task, newStatus) => {
        try {
            const updatedTask = { ...task, status: newStatus };
            await updateTask(updatedTask);
            loadTasks();
        } catch {
            alert("Error updating status");
        }
    };

    const handleAssigneeChange = async (task, newAssigneeId) => {
        try {
            const updatedTask = { ...task, executorId: newAssigneeId || null };
            await updateTask(updatedTask);
            loadTasks();
        } catch {
            alert("Error updating assignee");
        }
    };

    const handleEdit = (task) => {
        setForm({
            id: task.id,
            title: task.title,
            comment: task.comment || "",
            priority: task.priority,
            status: task.status,
            projectId: task.projectId,
            authorId: task.authorId,
            executorId: task.executorId || ""
        });
        setIsEditing(true);
        setShowCreateForm(true);
        setErrors({});
    };

    const resetForm = () => {
        setForm({
            id: null, title: "", comment: "", priority: 1, status: 0,
            projectId: "", authorId: "", executorId: ""
        });
        setIsEditing(false);
        setShowCreateForm(false);
        setErrors({});
    };

    const filteredTasks = tasks.filter(task =>
        task.title.toLowerCase().includes(searchTerm.toLowerCase())
    );

    return (
        <Layout title="Tasks">
            <Box sx={{ maxWidth: 1400, mx: "auto", px: 2 }}>
                <Paper elevation={0} sx={{ p: 2, mb: 3, borderRadius: 3, bgcolor: 'background.default' }}>
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2, alignItems: 'center' }}>
                        <Box sx={{ flexGrow: 1, minWidth: { xs: '100%', sm: 'auto' } }}>
                            <TextField
                                fullWidth
                                placeholder="Search tasks..."
                                value={searchTerm}
                                onChange={(e) => setSearchTerm(e.target.value)}
                                sx={{ '& .MuiOutlinedInput-root': { borderRadius: 3, bgcolor: 'background.paper' } }}
                            />
                        </Box>
                        <Box sx={{ width: { xs: 'calc(50% - 8px)', sm: 'auto' } }}>
                            <Autocomplete
                                options={projects}
                                getOptionLabel={(o) => o.projectName}
                                value={filterProject}
                                onChange={(e, v) => setFilterProject(v)}
                                renderInput={(params) => (
                                    <TextField
                                        {...params}
                                        label="Project"
                                        sx={{ width: 200, '& .MuiOutlinedInput-root': { borderRadius: 3, bgcolor: 'background.paper' } }}
                                    />
                                )}
                                isOptionEqualToValue={(o, v) => o.id === v.id}
                            />
                        </Box>
                        <Box sx={{ width: { xs: 'calc(50% - 8px)', sm: 'auto' } }}>
                            <Autocomplete
                                options={statusOptions}
                                getOptionLabel={(o) => o.label}
                                value={filterStatus}
                                onChange={(e, v) => setFilterStatus(v)}
                                disableClearable
                                renderInput={(params) => (
                                    <TextField
                                        {...params}
                                        label="Status"
                                        sx={{ width: 150, '& .MuiOutlinedInput-root': { borderRadius: 3, bgcolor: 'background.paper' } }}
                                    />
                                )}
                                isOptionEqualToValue={(o, v) => o.value === v.value}
                            />
                        </Box>
                        <Box sx={{ width: { xs: 'calc(50% - 8px)', sm: 'auto' } }}>
                            <Autocomplete
                                options={sortOptions}
                                getOptionLabel={(o) => o.label}
                                value={sortBy}
                                onChange={(e, v) => setSortBy(v)}
                                disableClearable
                                renderInput={(params) => (
                                    <TextField
                                        {...params}
                                        label="Sort By"
                                        sx={{ width: 120, '& .MuiOutlinedInput-root': { borderRadius: 3, bgcolor: 'background.paper' } }}
                                    />
                                )}
                                isOptionEqualToValue={(o, v) => o.value === v.value}
                            />
                        </Box>
                        <Box sx={{ marginLeft: 'auto' }}>
                            {canCreateTask && (
                                <Button
                                    variant="contained"
                                    startIcon={<AddIcon />}
                                    onClick={() => setShowCreateForm(true)}
                                    sx={{ borderRadius: 3, px: 3 }}
                                >
                                    New Task
                                </Button>
                            )}
                        </Box>
                    </Box>
                </Paper>

                {showCreateForm && canCreateTask && (
                    <Fade in={showCreateForm}>
                        <Paper elevation={2} sx={{ p: 3, mb: 3, borderRadius: 3 }}>
                            <Typography variant="h6" gutterBottom>
                                {isEditing ? "Edit Task" : "Create New Task"}
                            </Typography>
                            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                                <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                                    <Box sx={{ flex: 1, minWidth: { xs: '100%', sm: 'calc(50% - 8px)' } }}>
                                        <TextField
                                            label="Title"
                                            fullWidth
                                            value={form.title}
                                            onChange={(e) => handleFieldChange("title", e.target.value)}
                                            onBlur={(e) => checkTitleUnique(e.target.value)}
                                            error={!!errors.title}
                                            helperText={errors.title || (isCheckingTitle ? "Checking..." : "")}
                                            required
                                        />
                                    </Box>
                                    <Box sx={{ flex: 1, minWidth: { xs: '100%', sm: 'calc(50% - 8px)' } }}>
                                        <FormControl fullWidth error={!!errors.projectId} required>
                                            <InputLabel>Project</InputLabel>
                                            <Select
                                                value={form.projectId}
                                                label="Project"
                                                onChange={(e) => handleFieldChange("projectId", e.target.value)}
                                                disabled={isEditing}
                                            >
                                                {projects.map(p => <MenuItem key={p.id} value={p.id}>{p.projectName}</MenuItem>)}
                                            </Select>
                                            {errors.projectId && <FormHelperText>{errors.projectId}</FormHelperText>}
                                        </FormControl>
                                    </Box>
                                </Box>
                                <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                                    <Box sx={{ flex: 1, minWidth: { xs: '100%', sm: 'calc(50% - 8px)' } }}>
                                        <FormControl fullWidth error={!!errors.authorId} required>
                                            <InputLabel>Author</InputLabel>
                                            <Select
                                                value={form.authorId}
                                                label="Author"
                                                onChange={(e) => handleFieldChange("authorId", e.target.value)}
                                                disabled={isEditing}
                                            >
                                                {employees.map(emp => <MenuItem key={emp.id} value={emp.id}>{emp.lastName} {emp.firstName}</MenuItem>)}
                                            </Select>
                                            {errors.authorId && <FormHelperText>{errors.authorId}</FormHelperText>}
                                        </FormControl>
                                    </Box>
                                    <Box sx={{ flex: 1, minWidth: { xs: '100%', sm: 'calc(50% - 8px)' } }}>
                                        <FormControl fullWidth>
                                            <InputLabel>Assignee</InputLabel>
                                            <Select
                                                value={form.executorId}
                                                label="Assignee"
                                                onChange={(e) => setForm({ ...form, executorId: e.target.value })}
                                            >
                                                <MenuItem value=""><em>Unassigned</em></MenuItem>
                                                {employees.map(emp => <MenuItem key={emp.id} value={emp.id}>{emp.lastName} {emp.firstName}</MenuItem>)}
                                            </Select>
                                        </FormControl>
                                    </Box>
                                </Box>
                                <Box sx={{ display: 'flex', gap: 2, flexWrap: 'wrap' }}>
                                    <Box sx={{ width: { xs: '100%', sm: 'calc(50% - 8px)' } }}>
                                        <TextField
                                            label="Priority"
                                            type="number"
                                            fullWidth
                                            value={form.priority}
                                            onChange={(e) => handleFieldChange("priority", e.target.value)}
                                            error={!!errors.priority}
                                            helperText={errors.priority}
                                            required
                                            slotProps={{ htmlInput: { min: 1, max: 10, step: 1 } }}
                                            disabled={isEditing && !isChief}
                                        />
                                    </Box>
                                    <Box sx={{ width: { xs: '100%', sm: 'calc(50% - 8px)' } }}>
                                        <FormControl fullWidth>
                                            <InputLabel>Status</InputLabel>
                                            <Select
                                                value={form.status}
                                                label="Status"
                                                onChange={(e) => setForm({ ...form, status: e.target.value })}
                                            >
                                                <MenuItem value={0}>ToDo</MenuItem>
                                                <MenuItem value={1}>In Progress</MenuItem>
                                                <MenuItem value={2}>Done</MenuItem>
                                            </Select>
                                        </FormControl>
                                    </Box>
                                </Box>
                                <Box>
                                    <TextField
                                        label="Comment"
                                        fullWidth
                                        multiline
                                        rows={2}
                                        value={form.comment}
                                        onChange={(e) => setForm({ ...form, comment: e.target.value })}
                                        disabled={isEditing && !isChief}
                                    />
                                </Box>
                            </Box>
                            <Stack direction="row" spacing={2} sx={{ justifyContent: "flex-end", mt: 2 }}>
                                <Button variant="outlined" onClick={resetForm}>Cancel</Button>
                                <Button variant="contained" onClick={handleSave} disabled={!isFormValid()}>
                                    {isEditing ? "Save Changes" : "Create Task"}
                                </Button>
                            </Stack>
                        </Paper>
                    </Fade>
                )}

                {isLoading ? (
                    <Box sx={{ display: "flex", justifyContent: "center", p: 4 }}>
                        <CircularProgress />
                    </Box>
                ) : filteredTasks.length === 0 ? (
                    <Paper sx={{ p: 4, textAlign: 'center', borderRadius: 3 }}>
                        <Typography color="text.secondary">No tasks found</Typography>
                    </Paper>
                ) : (
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2 }}>
                        {filteredTasks.map((task) => {
                            const project = projects.find(p => p.id === task.projectId);
                            const assignee = employees.find(e => e.id === task.executorId);
                            const canEdit = isChief;
                            const canDelete = canDeleteTask;
                            const canChange = canChangeStatus();
                            const canChangeAss = canChangeAssignee();

                            return (
                                <Box key={task.id} sx={{ width: { xs: '100%', sm: 'calc(50% - 8px)', md: 'calc(33.333% - 16px)' } }}>
                                    <Card
                                        elevation={1}
                                        sx={{
                                            borderRadius: 3,
                                            transition: 'all 0.2s',
                                            '&:hover': { boxShadow: 4, transform: 'translateY(-2px)' },
                                            borderLeft: 6,
                                            borderColor: `${STATUS_MAP[task.status]?.color}.main`
                                        }}
                                    >
                                        <CardContent sx={{ pb: 1 }}>
                                            <Stack direction="row" sx={{ justifyContent: "space-between", alignItems: "flex-start" }}>
                                                <Typography variant="h6" fontWeight="bold" sx={{ mb: 1 }}>
                                                    {task.title}
                                                </Typography>
                                                <Chip
                                                    label={task.priority}
                                                    size="small"
                                                    color={priorityColors[task.priority] || "default"}
                                                    icon={<FlagOutlined />}
                                                />
                                            </Stack>

                                            {project && (
                                                <Typography variant="body2" color="text.secondary" gutterBottom>
                                                    {project.projectName}
                                                </Typography>
                                            )}

                                            {task.comment && (
                                                <Typography variant="body2" color="text.secondary" sx={{ mb: 1, fontStyle: 'italic' }}>
                                                    {task.comment.length > 60 ? task.comment.slice(0, 60) + '...' : task.comment}
                                                </Typography>
                                            )}

                                            <Stack direction="row" sx={{ alignItems: "center", spacing: 1, mt: 1 }}>
                                                <Chip
                                                    icon={STATUS_MAP[task.status]?.icon}
                                                    label={STATUS_MAP[task.status]?.label}
                                                    color={STATUS_MAP[task.status]?.color}
                                                    size="small"
                                                />
                                                {assignee ? (
                                                    <Tooltip title={assignee.email}>
                                                        <Chip
                                                            icon={<PersonOutline />}
                                                            label={`${assignee.lastName} ${assignee.firstName}`}
                                                            variant="outlined"
                                                            size="small"
                                                        />
                                                    </Tooltip>
                                                ) : (
                                                    <Chip
                                                        icon={<PersonOutline />}
                                                        label="Unassigned"
                                                        variant="outlined"
                                                        size="small"
                                                        disabled
                                                    />
                                                )}
                                            </Stack>
                                        </CardContent>
                                        <Divider />
                                        <CardActions sx={{ justifyContent: 'space-between', px: 2, py: 1 }}>
                                            <Stack direction="row" spacing={1}>
                                                {canChange && !canEdit && (
                                                    <FormControl size="small" sx={{ minWidth: 100 }}>
                                                        <Select
                                                            value={task.status}
                                                            onChange={(e) => handleStatusChange(task, e.target.value)}
                                                            variant="standard"
                                                            disableUnderline
                                                            sx={{ fontSize: '0.875rem' }}
                                                        >
                                                            <MenuItem value={0}>ToDo</MenuItem>
                                                            <MenuItem value={1}>In Progress</MenuItem>
                                                            <MenuItem value={2}>Done</MenuItem>
                                                        </Select>
                                                    </FormControl>
                                                )}
                                                {canChangeAss && !canEdit && (
                                                    <FormControl size="small" sx={{ minWidth: 120 }}>
                                                        <Select
                                                            value={task.executorId || ""}
                                                            onChange={(e) => handleAssigneeChange(task, e.target.value)}
                                                            variant="standard"
                                                            displayEmpty
                                                            disableUnderline
                                                            sx={{ fontSize: '0.875rem' }}
                                                        >
                                                            <MenuItem value=""><em>Unassigned</em></MenuItem>
                                                            {employees.map(emp => <MenuItem key={emp.id} value={emp.id}>{emp.lastName} {emp.firstName}</MenuItem>)}
                                                        </Select>
                                                    </FormControl>
                                                )}
                                            </Stack>
                                            <Stack direction="row">
                                                {canEdit && (
                                                    <IconButton size="small" onClick={() => handleEdit(task)}>
                                                        <EditIcon fontSize="small" />
                                                    </IconButton>
                                                )}
                                                {canDelete && (
                                                    <IconButton size="small" color="error" onClick={() => handleDelete(task.id)}>
                                                        <DeleteIcon fontSize="small" />
                                                    </IconButton>
                                                )}
                                            </Stack>
                                        </CardActions>
                                    </Card>
                                </Box>
                            );
                        })}
                    </Box>
                )}
            </Box>
        </Layout>
    );
}