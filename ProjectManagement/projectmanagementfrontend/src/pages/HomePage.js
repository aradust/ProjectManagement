import { useNavigate } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";
import {
  Button, Typography, Box, Card, CardContent,
  AppBar, Toolbar
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import ListAltIcon from "@mui/icons-material/ListAlt";
import PeopleIcon from "@mui/icons-material/People";
import AssignmentIcon from "@mui/icons-material/Assignment";
import LogoutIcon from "@mui/icons-material/Logout";

export default function HomePage() {
  const navigate = useNavigate();
  const { hasRole, logout, user } = useAuth();

  const allCards = [
    {
      condition: hasRole("Chief"),
      title: "Create Project",
      description: "Fill out the form to create a new project and assign a team",
      icon: <AddIcon sx={{ fontSize: 60, color: "primary.main", mb: 2 }} />,
      buttonText: "Start",
      buttonVariant: "contained",
      buttonColor: "primary",
      path: "/create",
    },
    {
      condition: true,
      title: "My Projects",
      description: "View the list of your projects and their statuses",
      icon: <ListAltIcon sx={{ fontSize: 60, color: "secondary.main", mb: 2 }} />,
      buttonText: "View Projects",
      buttonVariant: "outlined",
      buttonColor: "secondary",
      path: "/projects",
    },
    {
      condition: hasRole("Chief"),
      title: "Employees",
      description: "Manage company employee accounts",
      icon: <PeopleIcon sx={{ fontSize: 60, color: "success.main", mb: 2 }} />,
      buttonText: "Manage",
      buttonVariant: "contained",
      buttonColor: "success",
      path: "/employees",
    },
    {
      condition: true,
      title: "Tasks",
      description: "View tasks and change their statuses",
      icon: <AssignmentIcon sx={{ fontSize: 60, color: "warning.main", mb: 2 }} />,
      buttonText: "Go to Tasks",
      buttonVariant: "outlined",
      buttonColor: "warning",
      path: "/tasks",
    },
  ];

  const visibleCards = allCards.filter(card => card.condition);
  const cardCount = visibleCards.length;

  const getCardWidth = () => {
    if (cardCount === 1) return { xs: '100%', sm: '100%', md: '66.66%' };
    if (cardCount === 2) return { xs: '100%', sm: '50%', md: '50%' };
    if (cardCount === 3) return { xs: '100%', sm: '50%', md: '33.33%' };
    return { xs: '100%', sm: '50%', md: '50%' };
  };

  const cardWidth = getCardWidth();

  return (
    <Box sx={{ flexGrow: 1 }}>
      <AppBar position="static" color="default" elevation={0} sx={{ borderBottom: "1px solid #e0e0e0" }}>
        <Toolbar sx={{ justifyContent: "space-between" }}>
          <Typography variant="h6" color="inherit">Dashboard</Typography>
          <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
            <Typography variant="body2" sx={{ display: { xs: 'none', sm: 'block' } }}>
              {user?.email}
            </Typography>
            <Button
              variant="outlined"
              color="error"
              startIcon={<LogoutIcon />}
              onClick={logout}
              size="small"
            >
              Sign Out
            </Button>
          </Box>
        </Toolbar>
      </AppBar>

      <Box sx={{ mt: 4, px: { xs: 2, md: 0 } }}>
        <Typography variant="h3" gutterBottom align="center" sx={{ fontWeight: 700 }}>
          Project Management
        </Typography>
        <Typography variant="h6" gutterBottom align="center" color="text.secondary" sx={{ mb: 5 }}>
          Welcome to the System
        </Typography>

        <Box sx={{ display: 'flex', flexWrap: 'wrap', justifyContent: 'center', gap: 4 }}>
          {visibleCards.map((card, index) => (
            <Box key={index} sx={{ width: cardWidth }}>
              <Card
                sx={{
                  height: "100%",
                  display: "flex",
                  flexDirection: "column",
                  cursor: "pointer",
                  transition: "0.3s",
                  "&:hover": { transform: "translateY(-5px)", boxShadow: 6 }
                }}
                onClick={() => navigate(card.path)}
              >
                <CardContent sx={{ textAlign: "center", py: 6, flexGrow: 1, display: "flex", flexDirection: "column", alignItems: "center" }}>
                  {card.icon}
                  <Typography variant="h5" gutterBottom fontWeight="bold">{card.title}</Typography>
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 3, flexGrow: 1 }}>
                    {card.description}
                  </Typography>
                  <Button
                    variant={card.buttonVariant}
                    color={card.buttonColor}
                    onClick={(e) => { e.stopPropagation(); navigate(card.path); }}
                    sx={card.buttonColor === "warning" ? { color: "warning.main", borderColor: "warning.main" } : {}}
                  >
                    {card.buttonText}
                  </Button>
                </CardContent>
              </Card>
            </Box>
          ))}
        </Box>
      </Box>
    </Box>
  );
}