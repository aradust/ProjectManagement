import { useState, useEffect } from "react";
import { useNavigate, Link, useLocation } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";
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
} from "@mui/material";
import { Visibility, VisibilityOff } from "@mui/icons-material";

export default function LoginPage() {
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [showPassword, setShowPassword] = useState(false);
    const [errors, setErrors] = useState({ email: "", password: "" });
    const [serverError, setServerError] = useState("");
    const [touched, setTouched] = useState({ email: false, password: false });

    const { loginAction, user } = useAuth();
    const navigate = useNavigate();
    const location = useLocation();

    useEffect(() => {
        if (user) {
            const from = location.state?.from?.pathname || "/";
            navigate(from, { replace: true });
        }
    }, [user, navigate, location]);

    const validateEmail = (value) => {
        if (!value) return "Email is required";
        if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
            return "Invalid email format";
        }
        return "";
    };

    const validatePassword = (value) => {
        if (!value) return "Password is required";
        if (value.length < 8) return "Min 8 characters";
        if (!/[A-Z]/.test(value)) return "Need uppercase letter";
        if (!/[a-z]/.test(value)) return "Need lowercase letter";
        if (!/[0-9]/.test(value)) return "Need number";
        if (!/^[A-Za-z0-9]+$/.test(value)) return "Only letters/numbers";
        return "";
    };

    const validate = (field, value) => {
        const error = field === "email"
            ? validateEmail(value)
            : validatePassword(value);

        setErrors((prev) => ({ ...prev, [field]: error }));
        return error;
    };

    const handleChange = (field, value) => {
        if (field === "email") setEmail(value);
        if (field === "password") setPassword(value);

        if (touched[field]) {
            validate(field, value);
        }
    };

    const handleBlur = (field) => {
        setTouched((prev) => ({ ...prev, [field]: true }));
        const value = field === "email" ? email : password;
        validate(field, value);
    };

    const handleSubmit = async (e) => {
        e.preventDefault();

        setTouched({ email: true, password: true });
        setServerError("");

        const emailError = validate("email", email);
        const passwordError = validate("password", password);

        if (emailError || passwordError) return;

        try {
            await loginAction(email, password);
        } catch (error) {
            let errorMessage = "Invalid email or password";

            if (error.status === 401) {
                errorMessage = "Invalid email or password";
            } else if (error.status === 404) {
                errorMessage = "User not found";
            } else if (error.message) {
                errorMessage = error.message;
            }

            setServerError(errorMessage);
        }
    };

    return (
        <Container maxWidth="sm">
            <Box sx={{ mt: 8, display: "flex", justifyContent: "center" }}>
                <Paper elevation={3} sx={{ p: 4, width: "100%" }}>
                    <Typography variant="h5" align="center" gutterBottom>
                        Sign In
                    </Typography>

                    <Box component="form" onSubmit={handleSubmit} noValidate>
                        <TextField
                            fullWidth
                            label="Email"
                            value={email}
                            onChange={(e) => handleChange("email", e.target.value)}
                            onBlur={() => handleBlur("email")}
                            error={touched.email && !!errors.email}
                            helperText={touched.email ? errors.email : ""}
                            margin="normal"
                        />

                        <TextField
                            fullWidth
                            label="Password"
                            type={showPassword ? "text" : "password"}
                            value={password}
                            onChange={(e) => handleChange("password", e.target.value)}
                            onBlur={() => handleBlur("password")}
                            error={touched.password && !!errors.password}
                            helperText={touched.password ? errors.password : ""}
                            margin="normal"
                            slotProps={{
                                input: {
                                    endAdornment: (
                                        <InputAdornment position="end">
                                            <IconButton onClick={() => setShowPassword((s) => !s)}>
                                                {showPassword ? <VisibilityOff /> : <Visibility />}
                                            </IconButton>
                                        </InputAdornment>
                                    ),
                                },
                            }}
                        />

                        {serverError && (
                            <Alert
                                severity="error"
                                sx={{ mt: 2 }}
                                onClose={() => setServerError("")}
                            >
                                {serverError}
                            </Alert>
                        )}

                        <Button
                            type="submit"
                            fullWidth
                            variant="contained"
                            sx={{ mt: 3 }}
                        >
                            Login
                        </Button>

                        <Button
                            fullWidth
                            variant="outlined"
                            component={Link}
                            to="/register"
                            sx={{ mt: 2 }}
                        >
                            Sign Up
                        </Button>
                    </Box>
                </Paper>
            </Box>
        </Container>
    );
}