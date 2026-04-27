import { useState, useEffect, useRef } from "react";
import { useNavigate, Link } from "react-router-dom";
import { register } from "../api/api";
import {
    Box,
    TextField,
    Button,
    Typography,
    Paper,
    Container,
    InputAdornment,
    IconButton,
    Alert,
    Snackbar,
} from "@mui/material";
import { Visibility, VisibilityOff } from "@mui/icons-material";

export default function RegisterPage() {
    const [formData, setFormData] = useState({
        email: "",
        password: "",
        firstName: "",
        lastName: "",
        middleName: ""
    });

    const [showPassword, setShowPassword] = useState(false);
    const [errors, setErrors] = useState({
        email: "",
        password: "",
        firstName: "",
        lastName: ""
    });
    const [serverError, setServerError] = useState("");
    const [touched, setTouched] = useState({
        email: false,
        password: false,
        firstName: false,
        lastName: false
    });

    const [snackbar, setSnackbar] = useState({
        open: false,
        message: "",
        severity: "success"
    });

    const navigate = useNavigate();
    const isMounted = useRef(true);

    useEffect(() => {
        return () => {
            isMounted.current = false;
        };
    }, []);

    const validateEmail = (value) => {
        if (!value) return "Email is required";
        if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) return "Invalid email format";
        return "";
    };

    const validatePassword = (value) => {
        if (!value) return "Password is required";
        if (value.length < 8) return "Password must be at least 8 characters";
        if (!/[A-Z]/.test(value)) return "Password must contain at least one uppercase letter";
        if (!/[a-z]/.test(value)) return "Password must contain at least one lowercase letter";
        if (!/[0-9]/.test(value)) return "Password must contain at least one number";
        if (!/^[A-Za-z0-9]+$/.test(value)) return "Password must contain only letters and numbers";
        return "";
    };

    const validateName = (value) => {
        if (!value.trim()) return "This field is required";
        if (!/^[a-zA-Z]+$/.test(value)) return "Only Latin letters allowed";
        if (value.length < 2 || value.length > 100) return "Length must be 2-100 characters";
        return "";
    };

    const validateField = (field, value) => {
        let error = "";
        switch (field) {
            case "email": error = validateEmail(value); break;
            case "password": error = validatePassword(value); break;
            case "firstName":
            case "lastName": error = validateName(value); break;
            default: break;
        }
        setErrors((prev) => ({ ...prev, [field]: error }));
        return error;
    };

    const handleChange = (e) => {
        const { name, value } = e.target;
        setFormData({ ...formData, [name]: value });
        if (touched[name]) {
            validateField(name, value);
        }
    };

    const handleBlur = (field) => {
        setTouched((prev) => ({ ...prev, [field]: true }));
        validateField(field, formData[field]);
    };

    const handleClickShowPassword = () => setShowPassword((show) => !show);
    const handleMouseDownPassword = (event) => event.preventDefault();

    const handleCloseSnackbar = () => {
        setSnackbar(prev => ({ ...prev, open: false }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        setTouched({ email: true, password: true, firstName: true, lastName: true });
        setServerError("");

        const emailError = validateField("email", formData.email);
        const passwordError = validateField("password", formData.password);
        const firstNameError = validateField("firstName", formData.firstName);
        const lastNameError = validateField("lastName", formData.lastName);

        if (emailError || passwordError || firstNameError || lastNameError) return;

        try {
            await register(formData);
            if (isMounted.current) {
                setSnackbar({
                    open: true,
                    message: "Registration successful! Please log in.",
                    severity: "success"
                });
                setTimeout(() => navigate("/login"), 2000);
            }
        } catch (error) {
            if (isMounted.current) {
                let errorMessage = "Registration failed. Please try again.";
                if (error.response) {
                    if (error.response.status === 409) {
                        errorMessage = "User with this email already exists.";
                    } else if (error.response.data && error.response.data.message) {
                        errorMessage = error.response.data.message;
                    }
                } else if (error.message) {
                    errorMessage = error.message;
                }
                setServerError(errorMessage);
                setSnackbar({
                    open: true,
                    message: errorMessage,
                    severity: "error"
                });
            }
        }
    };

    return (
        <Container maxWidth="sm">
            <Box sx={{ mt: 8, display: "flex", flexDirection: "column", alignItems: "center" }}>
                <Paper elevation={3} sx={{ p: 4, width: "100%" }}>
                    <Typography component="h1" variant="h5" align="center" gutterBottom>
                        Sign Up
                    </Typography>

                    <Box component="form" onSubmit={handleSubmit} noValidate sx={{ mt: 1 }}>
                        <TextField
                            margin="normal"
                            required
                            fullWidth
                            label="Email Address"
                            name="email"
                            autoComplete="email"
                            autoFocus
                            value={formData.email}
                            onChange={handleChange}
                            onBlur={() => handleBlur("email")}
                            error={touched.email && !!errors.email}
                            helperText={touched.email ? errors.email : ""}
                        />

                        <TextField
                            margin="normal"
                            required
                            fullWidth
                            name="password"
                            label="Password"
                            type={showPassword ? "text" : "password"}
                            autoComplete="new-password"
                            value={formData.password}
                            onChange={handleChange}
                            onBlur={() => handleBlur("password")}
                            error={touched.password && !!errors.password}
                            helperText={touched.password ? errors.password : ""}
                            slotProps={{
                                input: {
                                    endAdornment: (
                                        <InputAdornment position="end">
                                            <IconButton
                                                aria-label="toggle password visibility"
                                                onClick={handleClickShowPassword}
                                                onMouseDown={handleMouseDownPassword}
                                                edge="end"
                                            >
                                                {showPassword ? <VisibilityOff /> : <Visibility />}
                                            </IconButton>
                                        </InputAdornment>
                                    ),
                                }
                            }}
                        />

                        <TextField
                            margin="normal"
                            required
                            fullWidth
                            name="firstName"
                            label="First Name"
                            autoComplete="given-name"
                            value={formData.firstName}
                            onChange={handleChange}
                            onBlur={() => handleBlur("firstName")}
                            error={touched.firstName && !!errors.firstName}
                            helperText={touched.firstName ? errors.firstName : ""}
                        />

                        <TextField
                            margin="normal"
                            required
                            fullWidth
                            name="lastName"
                            label="Last Name"
                            autoComplete="family-name"
                            value={formData.lastName}
                            onChange={handleChange}
                            onBlur={() => handleBlur("lastName")}
                            error={touched.lastName && !!errors.lastName}
                            helperText={touched.lastName ? errors.lastName : ""}
                        />

                        <TextField
                            margin="normal"
                            fullWidth
                            name="middleName"
                            label="Middle Name (Optional)"
                            autoComplete="additional-name"
                            value={formData.middleName}
                            onChange={handleChange}
                        />

                        {serverError && (
                            <Alert severity="error" sx={{ mt: 2, mb: 1 }} onClose={() => setServerError("")}>
                                {serverError}
                            </Alert>
                        )}

                        <Button
                            type="submit"
                            fullWidth
                            variant="contained"
                            sx={{ mt: 3, mb: 2 }}
                        >
                            Sign Up
                        </Button>

                        <Button
                            fullWidth
                            variant="outlined"
                            component={Link}
                            to="/login"
                            sx={{ mt: 1 }}
                        >
                            Already have an account? Sign In
                        </Button>
                    </Box>
                </Paper>
            </Box>

            <Snackbar
                open={snackbar.open}
                autoHideDuration={4000}
                onClose={handleCloseSnackbar}
                anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
            >
                <Alert onClose={handleCloseSnackbar} severity={snackbar.severity} sx={{ width: "100%" }}>
                    {snackbar.message}
                </Alert>
            </Snackbar>
        </Container>
    );
}