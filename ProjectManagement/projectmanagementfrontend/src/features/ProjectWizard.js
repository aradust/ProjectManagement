import { useState, useEffect } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import {
  Button, Stack, Alert, Box, IconButton, Tooltip, CircularProgress, Typography,
  Stepper, Step, StepLabel
} from "@mui/material";
import HomeIcon from "@mui/icons-material/Home";
import Step1 from "./steps/Step1MainInfo";
import Step2 from "./steps/Step2Companies";
import Step3 from "./steps/Step3Manager";
import Step4 from "./steps/Step4Employees";
import Step5 from "./steps/Step5Documents";
import dayjs from "dayjs";
import { useAuth } from "../contexts/AuthContext";
import {
  fetchProjectById,
  fetchProjectEmployees,
  fetchEmployees,
  createProject,
  updateProject,
  uploadDocument,
  checkProjectNameExists, // Импортируем функцию проверки
} from "../api/api.js";

const steps = ["Main Info", "Companies", "Manager", "Employees", "Documents"];

export default function ProjectWizard() {
  const navigate = useNavigate();
  const location = useLocation();
  const { hasRole } = useAuth();

  const isEditMode = location.state?.mode === "edit";
  const editProjectId = location.state?.projectId;
  const isManagerEdit = isEditMode && hasRole("Manager") && !hasRole("Chief");

  const [step, setStep] = useState(1);
  const [error, setError] = useState("");
  const [isLoadingData, setIsLoadingData] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [step1HasError, setStep1HasError] = useState(false);
  const [isCheckingName, setIsCheckingName] = useState(false); // Состояние проверки

  const [formData, setFormData] = useState({
    step1: { projectName: "", startDate: null, endDate: null, priority: null },
    step2: { clientCompanyName: "", executorCompanyName: "" },
    step3: { manager: null },
    step4: { employees: [] },
    step5: { documents: [], pendingDocuments: [] }
  });

  useEffect(() => {
    const loadProjectData = async () => {
      if (isEditMode && editProjectId) {
        setIsLoadingData(true);
        try {
          const projectData = await fetchProjectById(editProjectId);
          const allEmployees = await fetchEmployees();
          const projectEmployees = await fetchProjectEmployees(editProjectId);

          const managerObj = allEmployees.find(e => e.id === projectData.projectManagerId) || null;

          setFormData({
            step1: {
              projectName: projectData.projectName,
              startDate: dayjs(projectData.projectStart),
              endDate: dayjs(projectData.projectEnd),
              priority: String(projectData.priority)
            },
            step2: {
              clientCompanyName: projectData.customerCompanyName,
              executorCompanyName: projectData.executorCompanyName
            },
            step3: { manager: managerObj },
            step4: { employees: projectEmployees },
            step5: { documents: [], pendingDocuments: [] }
          });

          if (isManagerEdit) {
            setStep(4);
          }
        } catch {
          setError("Could not load project data");
        } finally {
          setIsLoadingData(false);
        }
      }
    };
    loadProjectData();
  }, [isEditMode, editProjectId, isManagerEdit]);

  const validateCurrentStep = () => {
    if (isManagerEdit) {
      const { employees } = formData.step4;
      if (!employees || employees.length === 0) {
        setError("Please select at least one employee");
        return false;
      }
      return true;
    }

    switch (step) {
      case 1: {
        const { projectName, startDate, endDate, priority } = formData.step1;
        if (!/^[a-zA-Z0-9_]{3,100}$/.test(projectName)) {
          setError("Invalid project name");
          return false;
        }
        if (!startDate || !endDate) {
          setError("Dates are required");
          return false;
        }
        const today = dayjs().startOf("day");
        if (dayjs(startDate).isBefore(today)) {
          setError("Start date must be today or later");
          return false;
        }
        if (dayjs(endDate).isBefore(dayjs(startDate))) {
          setError("End date must be after start date");
          return false;
        }
        const priorityNum = Number(priority);
        if (!Number.isInteger(priorityNum) || priorityNum < 1 || priorityNum > 10) {
          setError("Priority must be integer 1–10");
          return false;
        }
        break;
      }
      case 2: {
        const { clientCompanyName, executorCompanyName } = formData.step2;
        const regex = /^[a-zA-Z0-9_ ]{3,100}$/;
        if (!regex.test(clientCompanyName.trim())) {
          setError("Invalid client company name");
          return false;
        }
        if (!regex.test(executorCompanyName.trim())) {
          setError("Invalid executor company name");
          return false;
        }
        break;
      }
      case 3: {
        if (!formData.step3.manager) {
          setError("Please select a manager");
          return false;
        }
        break;
      }
      case 4: {
        if (!formData.step4.employees || formData.step4.employees.length === 0) {
          setError("Please select at least one employee");
          return false;
        }
        break;
      }
      case 5: {
        break;
      }
      default: return false;
    }
    return true;
  };

  const next = () => setStep(prev => prev + 1);
  const back = () => setStep(prev => prev - 1);

  const handleNext = async () => {
    setError("");

    // Базовая валидация полей
    if (!validateCurrentStep()) {
      return;
    }

    // Дополнительная проверка уникальности имени для шага 1
    if (step === 1) {
      const { projectName } = formData.step1;

      setIsCheckingName(true);
      try {
        const exists = await checkProjectNameExists(projectName, editProjectId);
        if (exists) {
          setError("Project with this name already exists");
          return;
        }
      } catch (error) {
        setError("Failed to validate project name. Please try again.");
        return;
      } finally {
        setIsCheckingName(false);
      }
    }

    // Если все проверки пройдены, переходим дальше
    next();
  };

  const handleFinish = async () => {
    setError("");
    if (!validateCurrentStep()) return;

    setIsSaving(true);
    try {
      let projectId = editProjectId;

      if (isEditMode) {
        await updateProject(projectId, formData, isManagerEdit);
      } else {
        const createdProject = await createProject(formData);
        projectId = createdProject.id;
      }

      if (!isEditMode) {
        const pendingDocs = formData.step5.pendingDocuments || [];
        for (const doc of pendingDocs) {
          const formDataFile = new FormData();
          formDataFile.append("file", doc.file);
          await uploadDocument(projectId, formDataFile);
        }
      }

      navigate("/success");
    } catch (error) {
      setError(error.message || "Failed to save project. Try again.");
    } finally {
      setIsSaving(false);
    }
  };

  const renderStep = () => {
    if (isManagerEdit) {
      return (
        <Step4
          formData={formData}
          setFormData={setFormData}
          mode="edit"
        />
      );
    }

    const stepProps = {
      formData,
      setFormData,
      projectId: editProjectId,
      mode: isEditMode ? "edit" : "create",
      onErrorChange: step === 1 ? setStep1HasError : undefined,
    };

    switch (step) {
      case 1: return <Step1 {...stepProps} />;
      case 2: return <Step2 {...stepProps} />;
      case 3: return <Step3 {...stepProps} />;
      case 4: return <Step4 {...stepProps} />;
      case 5: return <Step5 {...stepProps} />;
      default: return null;
    }
  };

  if (isLoadingData) {
    return (
      <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', height: '80vh' }}>
        <CircularProgress />
        <Typography sx={{ mt: 2 }}>Loading project data...</Typography>
      </Box>
    );
  }

  return (
    <Box sx={{ position: 'relative', paddingBottom: '60px' }}>
      <Box sx={{ position: 'absolute', top: 0, right: 0, zIndex: 10 }}>
        <Tooltip title="Go Home">
          <IconButton color="primary" onClick={() => navigate("/")}>
            <HomeIcon />
          </IconButton>
        </Tooltip>
      </Box>

      <Typography variant="h4" align="center" gutterBottom sx={{ mt: 2 }}>
        {isEditMode ? "Edit Project" : "Create New Project"}
      </Typography>

      {!isManagerEdit && (
        <Box sx={{ mb: 3 }}>
          <Stepper activeStep={step - 1} alternativeLabel>
            {steps.map((label) => (
              <Step key={label}>
                <StepLabel>{label}</StepLabel>
              </Step>
            ))}
          </Stepper>
        </Box>
      )}

      {renderStep()}

      <Stack spacing={2} sx={{ mt: 4, position: 'fixed', bottom: 0, left: 0, right: 0, bgcolor: 'background.paper', p: 2, boxShadow: 3, zIndex: 5 }}>
        {error && (
          <Alert severity="error" onClose={() => setError("")}>
            {error}
          </Alert>
        )}

        <Stack direction="row" spacing={2} sx={{ justifyContent: "center" }}>
          {!isManagerEdit && (
            <Button variant="outlined" onClick={back} disabled={step === 1 || isSaving || isCheckingName}>
              Back
            </Button>
          )}

          {(step === 5 || isManagerEdit) ? (
            <Button
              variant="contained"
              color="success"
              onClick={handleFinish}
              disabled={isSaving}
            >
              {isSaving ? "Saving..." : (isEditMode ? "Update Project" : "Finish")}
            </Button>
          ) : (
            <Button
              variant="contained"
              onClick={handleNext}
              disabled={isSaving || isCheckingName || (step === 1 && step1HasError)}
            >
              {isCheckingName ? "Checking..." : "Next"}
            </Button>
          )}
        </Stack>
      </Stack>
    </Box>
  );
}