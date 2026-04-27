import { useState, useEffect, useRef } from "react";
import {
    Stack, Typography, Box, CircularProgress, IconButton,
    List, ListItem, ListItemText, ListItemSecondaryAction, Alert,
} from "@mui/material";
import DeleteIcon from "@mui/icons-material/Delete";
import CloudUploadIcon from "@mui/icons-material/CloudUpload";
import { getProjectDocuments, uploadDocument, deleteDocument } from "../../api/api";

export default function Step5Documents({ formData, setFormData, projectId, mode, readOnly = false }) {
    const [documents, setDocuments] = useState([]);
    const [isLoading, setIsLoading] = useState(false);
    const [uploading, setUploading] = useState(false);
    const [error, setError] = useState("");
    const inputRef = useRef(null);
    const isEditMode = mode === "edit" && projectId != null;

    useEffect(() => {
        if (isEditMode) loadDocuments();
    }, [projectId, isEditMode]);

    const loadDocuments = async () => {
        setIsLoading(true);
        setError("");
        try {
            const data = await getProjectDocuments(projectId);
            setDocuments(data);
        } catch {
            setError("Failed to load documents. Please refresh.");
        } finally {
            setIsLoading(false);
        }
    };

    const handleFiles = async (files) => {
        if (readOnly || !files || files.length === 0) return;
        if (isEditMode) {
            setUploading(true);
            setError("");
            try {
                for (const file of files) {
                    const formData = new FormData();
                    formData.append("file", file);
                    const uploaded = await uploadDocument(projectId, formData);
                    setDocuments(prev => [...prev, uploaded]);
                }
            } catch {
                setError("Failed to upload one or more files.");
            } finally {
                setUploading(false);
            }
        } else {
            const fileArray = Array.from(files).map(file => ({ file, name: file.name, size: file.size }));
            setFormData({
                ...formData,
                step5: {
                    ...formData.step5,
                    pendingDocuments: [...(formData.step5.pendingDocuments || []), ...fileArray],
                },
            });
        }
    };

    const handleDelete = async (doc) => {
        if (readOnly) return;
        if (isEditMode && doc.id) {
            try {
                await deleteDocument(doc.id);
                setDocuments(prev => prev.filter(d => d.id !== doc.id));
            } catch {
                setError("Failed to delete document.");
            }
        } else {
            const updated = (formData.step5.pendingDocuments || []).filter((_, i) => i !== doc.index);
            setFormData({
                ...formData,
                step5: { ...formData.step5, pendingDocuments: updated },
            });
        }
    };

    const onDrop = (e) => {
        e.preventDefault();
        handleFiles(e.dataTransfer.files);
    };

    const onDragOver = (e) => e.preventDefault();

    const handleFileSelect = (e) => {
        handleFiles(e.target.files);
        e.target.value = null;
    };

    const formatFileSize = (bytes) => {
        if (bytes < 1024) return `${bytes} B`;
        if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
        return `${(bytes / (1024 * 1024)).toFixed(2)} MB`;
    };

    const existingDocs = isEditMode ? documents : [];
    const pendingDocs = !isEditMode
        ? (formData.step5.pendingDocuments || []).map((doc, idx) => ({ ...doc, index: idx }))
        : [];

    return (
        <Stack spacing={3}>
            <Typography variant="h5" align="center">Step 5: Documents</Typography>
            {error && <Alert severity="error" onClose={() => setError("")}>{error}</Alert>}
            <Box
                onDrop={onDrop}
                onDragOver={onDragOver}
                onClick={() => !readOnly && inputRef.current.click()}
                sx={{
                    border: "2px dashed #1976d2",
                    borderRadius: 2,
                    p: 4,
                    textAlign: "center",
                    cursor: readOnly ? "default" : "pointer",
                    backgroundColor: "#f5f5f5",
                    transition: "background-color 0.2s",
                    "&:hover": { backgroundColor: readOnly ? "#f5f5f5" : "#e3f2fd" },
                }}
            >
                <CloudUploadIcon sx={{ fontSize: 48, color: readOnly ? "#aaa" : "#1976d2", mb: 1 }} />
                <Typography variant="body1" color="textSecondary">
                    {readOnly ? "Document upload is disabled" : "Drag & Drop files here or click to upload"}
                </Typography>
                <Typography variant="caption" color="textSecondary">
                    Supported formats: any
                </Typography>
                {uploading && <Box sx={{ mt: 1 }}><CircularProgress size={24} /></Box>}
            </Box>
            <input ref={inputRef} type="file" multiple hidden onChange={handleFileSelect} disabled={readOnly} />
            {isLoading ? (
                <Box sx={{ display: "flex", justifyContent: "center", py: 4 }}><CircularProgress /></Box>
            ) : (
                (existingDocs.length > 0 || pendingDocs.length > 0) && (
                    <Box>
                        <Typography variant="subtitle1" gutterBottom fontWeight="bold">
                            Uploaded Documents ({existingDocs.length + pendingDocs.length})
                        </Typography>
                        <List dense>
                            {existingDocs.map(doc => (
                                <ListItem key={doc.id} divider>
                                    <ListItemText primary={doc.originalName} secondary={formatFileSize(doc.size)} />
                                    <ListItemSecondaryAction>
                                        <IconButton edge="end" onClick={() => handleDelete(doc)} color="error" disabled={readOnly}>
                                            <DeleteIcon />
                                        </IconButton>
                                    </ListItemSecondaryAction>
                                </ListItem>
                            ))}
                            {pendingDocs.map(doc => (
                                <ListItem key={doc.index} divider>
                                    <ListItemText primary={doc.name} secondary={formatFileSize(doc.size)} />
                                    <ListItemSecondaryAction>
                                        <IconButton edge="end" onClick={() => handleDelete(doc)} color="error" disabled={readOnly}>
                                            <DeleteIcon />
                                        </IconButton>
                                    </ListItemSecondaryAction>
                                </ListItem>
                            ))}
                        </List>
                    </Box>
                )
            )}
        </Stack>
    );
}