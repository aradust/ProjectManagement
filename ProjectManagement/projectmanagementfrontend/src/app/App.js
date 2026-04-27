import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { AuthProvider, useAuth } from "../contexts/AuthContext";
import { RoleRoute } from "../components/RoleRoute";
import HomePage from "../pages/HomePage";
import ProjectsPage from "../pages/ProjectsPage";
import SuccessPage from "../pages/SuccessPage";
import TaskPage from "../pages/TaskPage";
import EmployeePage from "../pages/EmployeePage";
import ProjectWizard from "../features/ProjectWizard";
import LoginPage from "../pages/LoginPage";
import RegisterPage from "../pages/RegisterPage";

const PublicRoute = ({ children }) => {
  const { user, loading } = useAuth();

  if (loading) return <div>Loading...</div>;

  if (user) {
    return <Navigate to="/" replace />;
  }

  return <>{children}</>;
};

export default function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route
            path="/login"
            element={
              <PublicRoute>
                <LoginPage />
              </PublicRoute>
            }
          />

          <Route
            path="/register"
            element={
              <PublicRoute>
                <RegisterPage />
              </PublicRoute>
            }
          />

          <Route
            path="/"
            element={
              <RoleRoute allowedRoles={["Chief", "Manager", "Employee"]}>
                <HomePage />
              </RoleRoute>
            }
          />

          <Route
            path="/projects"
            element={
              <RoleRoute allowedRoles={["Chief", "Manager", "Employee"]}>
                <ProjectsPage />
              </RoleRoute>
            }
          />

          <Route
            path="/create"
            element={
              <RoleRoute allowedRoles={["Chief", "Manager"]}>
                <ProjectWizard />
              </RoleRoute>
            }
          />

          <Route
            path="/employees"
            element={
              <RoleRoute allowedRoles={["Chief"]}>
                <EmployeePage />
              </RoleRoute>
            }
          />

          <Route
            path="/tasks"
            element={
              <RoleRoute allowedRoles={["Chief", "Manager", "Employee"]}>
                <TaskPage />
              </RoleRoute>
            }
          />

          <Route
            path="/success"
            element={
              <RoleRoute allowedRoles={["Chief", "Manager", "Employee"]}>
                <SuccessPage />
              </RoleRoute>
            }
          />

          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}