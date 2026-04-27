import { AppBar, Toolbar, Typography, Button, Box } from "@mui/material";
import { useNavigate } from "react-router-dom";

export default function Layout({ children, title }) {
    const navigate = useNavigate();

    return (
        <Box sx={{ flexGrow: 1 }}>
            <AppBar position="static" color="default" elevation={0} sx={{ borderBottom: "1px solid #e0e0e0" }}>
                <Toolbar sx={{ justifyContent: "space-between" }}>
                    <Typography variant="h6">{title}</Typography>
                    <Button variant="outlined" onClick={() => navigate("/")}>Home</Button>
                </Toolbar>
            </AppBar>
            <Box sx={{ p: 3 }}>{children}</Box>
        </Box>
    );
}