import { Stack, Typography, Autocomplete, TextField } from "@mui/material";
import { useEffect, useState } from "react";
import { fetchEmployees } from "../../api/api.js";

export default function Step4Employees({ formData, setFormData, readOnly = false }) {
    const [employees, setEmployees] = useState([]);
    const step4 = formData.step4;

    useEffect(() => {
        fetchEmployees()
            .then(data => setEmployees(data))
            .catch(() => { });
    }, []);

    return (
        <Stack spacing={2}>
            <Typography variant="h5" align="center">Step 4: Employees</Typography>
            <Autocomplete
                multiple
                options={employees}
                getOptionLabel={(option) => option ? `${option.lastName} ${option.firstName}` : ""}
                value={step4.employees || []}
                isOptionEqualToValue={(option, value) => option.id === value.id}
                onChange={(e, newValue) => {
                    if (readOnly) return;
                    setFormData(prev => ({ ...prev, step4: { ...prev.step4, employees: newValue } }));
                }}
                renderInput={(params) => <TextField {...params} label="Select employees" placeholder="Employees" />}
                disabled={readOnly}
            />
        </Stack>
    );
}