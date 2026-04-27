import { TextField, Stack, Typography } from "@mui/material";
import { DatePicker } from "@mui/x-date-pickers/DatePicker";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import { useState, useEffect } from "react";
import dayjs from "dayjs";

export default function Step1MainInfo({
  formData,
  setFormData,
  readOnly = false,
  onErrorChange
}) {
  const step1 = formData.step1;
  const [errors, setErrors] = useState({});
  const today = dayjs().startOf("day");

  useEffect(() => {
    if (onErrorChange) {
      const hasError = Object.values(errors).some(err => err !== "");
      onErrorChange(hasError);
    }
  }, [errors, onErrorChange]);

  const validatePriority = (value) => {
    if (value === "") return "Required";
    if (!/^\d+$/.test(value)) return "Only digits allowed";
    const num = Number(value);
    if (!Number.isInteger(num)) return "Must be integer";
    if (num < 0) return "Cannot be negative";
    if (num === 0) return "Must be at least 1";
    if (num > 10) return "Max 10";
    return "";
  };

  const validate = (field, value) => {
    let error = "";

    if (field === "projectName") {
      if (!value) {
        error = "Required";
      } else if (!/^[a-zA-Z0-9_]+$/.test(value)) {
        error = "Only latin letters, digits and _";
      } else if (value.length < 3 || value.length > 100) {
        error = "Length must be 3–100";
      }
    }

    if (field === "startDate") {
      if (!value) {
        error = "Required";
      } else if (!dayjs(value).isValid()) {
        error = "Invalid date";
      } else if (dayjs(value).isBefore(today)) {
        error = "Date must be today or later";
      }
    }

    if (field === "endDate") {
      if (!value) {
        error = "Required";
      } else if (!dayjs(value).isValid()) {
        error = "Invalid date";
      } else if (dayjs(value).isBefore(today)) {
        error = "Date must be today or later";
      } else if (
        step1.startDate &&
        dayjs(step1.startDate).isValid() &&
        dayjs(value).isBefore(step1.startDate)
      ) {
        error = "End date must be after start date";
      }
    }

    if (field === "priority") {
      error = validatePriority(value);
    }

    setErrors(prev => ({ ...prev, [field]: error }));
  };

  const handleChange = (field, value) => {
    if (readOnly) return;
    if (field === "priority" && /^\d+$/.test(value)) {
      value = String(Number(value));
    }

    const newStep1 = { ...step1, [field]: value };
    setFormData({ ...formData, step1: newStep1 });
    validate(field, value);

    if (field === "startDate" && newStep1.endDate) {
      validate("endDate", newStep1.endDate);
    }
  };

  return (
    <LocalizationProvider dateAdapter={AdapterDayjs}>
      <Stack spacing={2}>
        <Typography variant="h5" align="center">Step 1: Main Info</Typography>
        <TextField
          label="Project name"
          value={step1.projectName}
          onChange={(e) => handleChange("projectName", e.target.value)}
          error={!!errors.projectName}
          helperText={errors.projectName}
          fullWidth
          disabled={readOnly}
        />
        <DatePicker
          label="Start date"
          value={step1.startDate}
          onChange={(value) => handleChange("startDate", value)}
          minDate={today}
          slotProps={{ textField: { error: !!errors.startDate, helperText: errors.startDate, disabled: readOnly } }}
        />
        <DatePicker
          label="End date"
          value={step1.endDate}
          onChange={(value) => handleChange("endDate", value)}
          minDate={today}
          slotProps={{ textField: { error: !!errors.endDate, helperText: errors.endDate, disabled: readOnly } }}
        />
        <TextField
          label="Priority (1-10)"
          type="text"
          value={step1.priority ?? ""}
          onChange={(e) => {
            const val = e.target.value;
            if (/^\d*$/.test(val)) handleChange("priority", val);
          }}
          error={!!errors.priority}
          helperText={errors.priority}
          fullWidth
          disabled={readOnly}
        />
      </Stack>
    </LocalizationProvider>
  );
}