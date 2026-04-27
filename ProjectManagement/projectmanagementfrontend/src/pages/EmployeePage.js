import { useState, useEffect, useCallback } from "react";
import { fetchEmployees, createEmployee, updateEmployee, deleteEmployee, checkEmployeeEmailExists } from "../api/api.js";
import {
    Box,
    TextField,
    Button,
    Stack,
    Typography,
    List,
    ListItem,
    ListItemText,
    IconButton,
    InputAdornment,
    CircularProgress,
    Divider,
    Paper,
    Tooltip,
    Select,
    MenuItem,
    FormControl,
    InputLabel,
    Chip,
    Snackbar,
} from "@mui/material";
import SearchIcon from "@mui/icons-material/Search";
import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import Visibility from "@mui/icons-material/Visibility";
import VisibilityOff from "@mui/icons-material/VisibilityOff";
import ContentCopyIcon from "@mui/icons-material/ContentCopy";
import Layout from "../components/Layout";
import { useAuth } from "../contexts/AuthContext";

const generateRandomPassword = (length = 12) => {
    const upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    const lower = "abcdefghijklmnopqrstuvwxyz";
    const digits = "0123456789";
    const all = upper + lower + digits;

    let password = "";
    password += upper[Math.floor(Math.random() * upper.length)];
    password += lower[Math.floor(Math.random() * lower.length)];
    password += digits[Math.floor(Math.random() * digits.length)];
    for (let i = password.length; i < length; i++) {
        password += all[Math.floor(Math.random() * all.length)];
    }
    return password.split('').sort(() => Math.random() - 0.5).join('');
};

export default function EmployeePage() {
    const { user, hasRole } = useAuth();
    const isChief = hasRole("Chief");

    const [employees, setEmployees] = useState([]);
    const [isLoading, setIsLoading] = useState(false);
    const [searchQuery, setSearchQuery] = useState("");

    const [form, setForm] = useState({
        firstName: "",
        lastName: "",
        middleName: "",
        email: "",
        password: "",
        role: "Employee"
    });
    const [errors, setErrors] = useState({});
    const [editingId, setEditingId] = useState(null);
    const [isCheckingEmail, setIsCheckingEmail] = useState(false);
    const [showPassword, setShowPassword] = useState(false);
    const [createdPassword, setCreatedPassword] = useState(null);
    const [snackbarOpen, setSnackbarOpen] = useState(false);

    useEffect(() => {
        if (isChief) loadEmployees();
    }, [searchQuery, isChief]);

    const loadEmployees = async () => {
        setIsLoading(true);
        try {
            const data = await fetchEmployees(searchQuery);
            setEmployees(data);
        } finally {
            setIsLoading(false);
        }
    };

    const validateField = (name, value) => {
        const nameRegex = /^[a-zA-Z]{2,50}$/;
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

        switch (name) {
            case "firstName":
                if (!value) return "Required";
                if (!nameRegex.test(value)) return "2–50 latin letters only";
                return "";
            case "lastName":
                if (!value) return "Required";
                if (!nameRegex.test(value)) return "2–50 latin letters only";
                return "";
            case "middleName":
                if (value && !nameRegex.test(value)) return "2–50 latin letters only";
                return "";
            case "email":
                if (!value) return "Required";
                if (!emailRegex.test(value)) return "Invalid email format";
                return "";
            case "password":
                if (!editingId) {
                    if (!value) return "Required";
                    if (value.length < 8) return "At least 8 characters";
                    if (!/[A-Z]/.test(value)) return "One uppercase letter";
                    if (!/[a-z]/.test(value)) return "One lowercase letter";
                    if (!/[0-9]/.test(value)) return "One number";
                }
                return "";
            case "role":
                if (!value) return "Required";
                return "";
            default:
                return "";
        }
    };

    const checkEmailUnique = useCallback(async (email) => {
        if (!email || validateField("email", email) !== "") return;
        setIsCheckingEmail(true);
        try {
            const exists = await checkEmployeeEmailExists(email, editingId);
            if (exists) {
                setErrors(prev => ({ ...prev, email: "Employee with this email already exists" }));
            } else {
                setErrors(prev => ({ ...prev, email: "" }));
            }
        } finally {
            setIsCheckingEmail(false);
        }
    }, [editingId]);

    const isFormValid = () => {
        const requiredFields = editingId
            ? ["firstName", "lastName", "email", "role"]
            : ["firstName", "lastName", "email", "password", "role"];
        return requiredFields.every(field => validateField(field, form[field]) === "")
            && !errors.email
            && !isCheckingEmail;
    };

    const handleFieldChange = (field, value) => {
        setForm(prev => ({ ...prev, [field]: value }));
        const error = validateField(field, value);
        setErrors(prev => ({ ...prev, [field]: error }));
    };

    const handleGeneratePassword = () => {
        const newPassword = generateRandomPassword(12);
        setForm(prev => ({ ...prev, password: newPassword }));
        setErrors(prev => ({ ...prev, password: "" }));
    };

    const handleCopyPassword = () => {
        if (form.password) {
            navigator.clipboard.writeText(form.password);
        }
    };

    const handleSave = async () => {
        if (isCheckingEmail) return;
        const fieldsToValidate = editingId
            ? ["firstName", "lastName", "email", "role"]
            : ["firstName", "lastName", "email", "password", "role"];
        const newErrors = {};
        fieldsToValidate.forEach(field => {
            newErrors[field] = validateField(field, form[field]);
        });
        setErrors(newErrors);
        if (Object.values(newErrors).some(err => err !== "") || errors.email) return;

        try {
            if (editingId) {
                const payload = {
                    id: editingId,
                    firstName: form.firstName,
                    lastName: form.lastName,
                    middleName: form.middleName || null,
                    email: form.email,
                    role: form.role
                };
                const updated = await updateEmployee(payload);
                setEmployees(employees.map(e => e.id === updated.id ? updated : e));
                resetForm();
            } else {
                const payload = {
                    firstName: form.firstName,
                    lastName: form.lastName,
                    middleName: form.middleName || null,
                    email: form.email,
                    password: form.password,
                    role: form.role
                };
                const created = await createEmployee(payload);
                setEmployees([...employees, created]);
                setCreatedPassword(form.password);
                setSnackbarOpen(true);
                resetForm();
            }
        } catch (e) {
            let errorMessage = "Error saving employee";
            if (e.response?.data?.message) {
                errorMessage = e.response.data.message;
            } else if (e.message) {
                errorMessage = e.message;
            }
            alert(errorMessage);
        }
    };

    const handleDelete = async (id) => {
        if (!confirm("Delete employee?")) return;
        try {
            await deleteEmployee(id);
            setEmployees(employees.filter(e => e.id !== id));
            if (editingId === id) resetForm();
        } catch (e) {
            let errorMessage = "Error deleting employee";
            if (e.response?.data?.message) {
                errorMessage = e.response.data.message;
            } else if (e.message) {
                errorMessage = e.message;
            }
            alert(errorMessage);
        }
    };

    const handleSelectEmployee = (emp) => {
        setForm({
            firstName: emp.firstName || "",
            lastName: emp.lastName || "",
            middleName: emp.middleName || "",
            email: emp.email || "",
            password: "",
            role: emp.role || "Employee"
        });
        setEditingId(emp.id);
        setErrors({});
    };

    const resetForm = () => {
        setForm({ firstName: "", lastName: "", middleName: "", email: "", password: "", role: "Employee" });
        setEditingId(null);
        setErrors({});
        setShowPassword(false);
    };

    const getRoleChipColor = (role) => {
        switch (role) {
            case "Chief": return "error";
            case "Manager": return "warning";
            default: return "default";
        }
    };

    const isSelfEdit = editingId === user?.id;

    if (!isChief) {
        return (
            <Layout title="Employees">
                <Box sx={{ maxWidth: 1200, mx: "auto", textAlign: "center", mt: 8 }}>
                    <Typography variant="h5" color="error">Access Denied</Typography>
                    <Typography variant="body1" sx={{ mt: 2 }}>
                        Only Chief can manage employees.
                    </Typography>
                </Box>
            </Layout>
        );
    }

    return (
        <Layout title="Employees">
            <Box sx={{ maxWidth: 1200, mx: "auto" }}>
                <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '1fr 1fr' }, gap: 4 }}>

                    <Paper sx={{ p: 3 }}>
                        <Typography variant="h6" gutterBottom color="primary">
                            {editingId ? "Edit Employee" : "Add New Employee"}
                        </Typography>

                        <Stack spacing={2}>
                            <TextField
                                label="Last Name"
                                value={form.lastName}
                                onChange={(e) => handleFieldChange("lastName", e.target.value)}
                                error={!!errors.lastName}
                                helperText={errors.lastName}
                                required
                            />
                            <TextField
                                label="First Name"
                                value={form.firstName}
                                onChange={(e) => handleFieldChange("firstName", e.target.value)}
                                error={!!errors.firstName}
                                helperText={errors.firstName}
                                required
                            />
                            <TextField
                                label="Middle Name"
                                value={form.middleName}
                                onChange={(e) => handleFieldChange("middleName", e.target.value)}
                                error={!!errors.middleName}
                                helperText={errors.middleName}
                            />
                            <TextField
                                label="Email"
                                type="email"
                                value={form.email}
                                onChange={(e) => handleFieldChange("email", e.target.value)}
                                onBlur={(e) => checkEmailUnique(e.target.value)}
                                error={!!errors.email}
                                helperText={errors.email || (isCheckingEmail ? "Checking..." : "")}
                                required
                            />

                            {!editingId && (
                                <>
                                    <TextField
                                        label="Password"
                                        type={showPassword ? "text" : "password"}
                                        value={form.password}
                                        onChange={(e) => handleFieldChange("password", e.target.value)}
                                        error={!!errors.password}
                                        helperText={errors.password}
                                        required
                                        slotProps={{
                                            input: {
                                                endAdornment: (
                                                    <InputAdornment position="end">
                                                        <Tooltip title="Show/Hide">
                                                            <IconButton onClick={() => setShowPassword(!showPassword)} edge="end">
                                                                {showPassword ? <VisibilityOff /> : <Visibility />}
                                                            </IconButton>
                                                        </Tooltip>
                                                        <Tooltip title="Copy">
                                                            <IconButton onClick={handleCopyPassword} edge="end">
                                                                <ContentCopyIcon />
                                                            </IconButton>
                                                        </Tooltip>
                                                    </InputAdornment>
                                                )
                                            }
                                        }}
                                    />
                                    <Button variant="outlined" onClick={handleGeneratePassword} sx={{ alignSelf: 'flex-start' }}>
                                        Generate Secure Password
                                    </Button>
                                </>
                            )}

                            <FormControl fullWidth required error={!!errors.role}>
                                <InputLabel>Role</InputLabel>
                                <Tooltip title={isSelfEdit ? "You cannot change your own role" : ""}>
                                    <Select
                                        value={form.role}
                                        label="Role"
                                        onChange={(e) => handleFieldChange("role", e.target.value)}
                                        disabled={isSelfEdit}
                                    >
                                        <MenuItem value="Employee">Employee</MenuItem>
                                        <MenuItem value="Manager">Manager</MenuItem>
                                    </Select>
                                </Tooltip>
                                {errors.role && (
                                    <Typography variant="caption" color="error">
                                        {errors.role}
                                    </Typography>
                                )}
                            </FormControl>

                            <Stack direction="row" spacing={2} sx={{ mt: 2 }}>
                                <Button
                                    variant="contained"
                                    fullWidth
                                    onClick={handleSave}
                                    color={editingId ? "warning" : "primary"}
                                    disabled={!isFormValid()}
                                >
                                    {editingId ? "Save Changes" : "Add Employee"}
                                </Button>
                                {editingId && (
                                    <Button variant="text" onClick={resetForm}>Cancel</Button>
                                )}
                            </Stack>
                        </Stack>
                    </Paper>

                    <Paper sx={{ p: 3, height: 'fit-content' }}>
                        <Typography variant="h6" gutterBottom>Employee List</Typography>

                        <TextField
                            fullWidth
                            placeholder="Search by name..."
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
                            sx={{ mb: 2 }}
                        />

                        <Divider sx={{ mb: 2 }} />

                        {isLoading ? (
                            <Box sx={{ display: 'flex', justifyContent: 'center', p: 3 }}>
                                <CircularProgress />
                            </Box>
                        ) : employees.length === 0 ? (
                            <Typography align="center" color="text.secondary">No employees found</Typography>
                        ) : (
                            <List>
                                {employees.map((emp) => {
                                    const isSelf = emp.id === user?.id;
                                    return (
                                        <ListItem
                                            key={emp.id}
                                            sx={{
                                                cursor: 'pointer',
                                                border: '1px solid #eee',
                                                mb: 1,
                                                borderRadius: 1,
                                                bgcolor: editingId === emp.id ? 'action.selected' : 'transparent'
                                            }}
                                            onClick={() => handleSelectEmployee(emp)}
                                        >
                                            <ListItemText
                                                primary={
                                                    <Stack direction="row" sx={{ alignItems: "center", spacing: 1 }}>
                                                        <Typography variant="body1">
                                                            {emp.lastName} {emp.firstName}
                                                        </Typography>
                                                        <Chip
                                                            label={emp.role}
                                                            size="small"
                                                            color={getRoleChipColor(emp.role)}
                                                        />
                                                    </Stack>
                                                }
                                                secondary={emp.email}
                                            />
                                            <IconButton edge="end" onClick={(e) => { e.stopPropagation(); handleSelectEmployee(emp); }}>
                                                <EditIcon fontSize="small" />
                                            </IconButton>
                                            <Tooltip title={isSelf ? "You cannot delete yourself" : ""}>
                                                <span>
                                                    <IconButton
                                                        edge="end"
                                                        onClick={(e) => { e.stopPropagation(); handleDelete(emp.id); }}
                                                        disabled={isSelf}
                                                        color={isSelf ? "default" : "error"}
                                                    >
                                                        <DeleteIcon fontSize="small" />
                                                    </IconButton>
                                                </span>
                                            </Tooltip>
                                        </ListItem>
                                    );
                                })}
                            </List>
                        )}
                    </Paper>
                </Box>
            </Box>

            <Snackbar
                open={snackbarOpen}
                autoHideDuration={6000}
                onClose={() => setSnackbarOpen(false)}
                message={`Employee created. Temporary password: ${createdPassword}`}
                action={
                    <Button color="inherit" size="small" onClick={() => navigator.clipboard.writeText(createdPassword)}>
                        Copy
                    </Button>
                }
            />
        </Layout>
    );
}