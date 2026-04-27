import { TextField, Stack, Typography } from "@mui/material";
import { useState } from "react";

export default function Step2Companies({ formData, setFormData, readOnly = false }) {
    const step2 = formData.step2;
    const [errors, setErrors] = useState({});

    const validate = (field, value) => {
        let error = "";
        const val = String(value).trim();
        if (val === "") error = "Required";
        else if (!/^[a-zA-Z0-9_ ]+$/.test(val)) error = "Only latin letters, digits, spaces and _";
        else if (val.length < 3 || val.length > 100) error = "Length must be 3–100";
        setErrors(prev => ({ ...prev, [field]: error }));
    };

    const handleChange = (field, value) => {
        if (readOnly) return;
        const newStep2 = { ...step2, [field]: value };
        setFormData({ ...formData, step2: newStep2 });
        validate(field, value);
    };

    return (
        <Stack spacing={2}>
            <Typography variant="h5" align="center">Step 2: Companies</Typography>
            <TextField
                label="Client company name"
                value={step2.clientCompanyName}
                onChange={(e) => handleChange("clientCompanyName", e.target.value)}
                error={!!errors.clientCompanyName}
                helperText={errors.clientCompanyName}
                fullWidth
                disabled={readOnly}
            />
            <TextField
                label="Executor company name"
                value={step2.executorCompanyName}
                onChange={(e) => handleChange("executorCompanyName", e.target.value)}
                error={!!errors.executorCompanyName}
                helperText={errors.executorCompanyName}
                fullWidth
                disabled={readOnly}
            />
        </Stack>
    );
}