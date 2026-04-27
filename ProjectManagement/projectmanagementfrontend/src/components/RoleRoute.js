import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";
import { Box, CircularProgress } from "@mui/material";

export const RoleRoute = ({ children, allowedRoles }) => {
    const { user, loading } = useAuth();
    const location = useLocation();

    if (loading) {
        return (
            <Box sx={{
                display: "flex",
                justifyContent: "center",
                alignItems: "center",
                height: "100vh"
            }}>
                <CircularProgress />
            </Box>
        );
    }

    if (!user) {
        if (location.pathname !== "/login") {
            return <Navigate to="/login" state={{ from: location }} replace />;
        }
        return children;
    }

    const hasAccess = allowedRoles.some(role => user.roles?.includes(role));

    if (!hasAccess) {
        return <Navigate to="/" replace />;
    }

    return children;
};