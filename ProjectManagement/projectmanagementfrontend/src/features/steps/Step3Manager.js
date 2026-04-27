import { Stack, Typography, Autocomplete, TextField } from "@mui/material";
import { useEffect, useState, useMemo } from "react";
import { fetchEmployees } from "../../api/api.js";

export default function Step3Manager({ formData, setFormData, readOnly = false }) {
    const [allEmployees, setAllEmployees] = useState([]);
    const step3 = formData.step3;

    useEffect(() => {
        fetchEmployees()
            .then(data => setAllEmployees(data))
            .catch(() => { });
    }, []);

    const managerEmployees = useMemo(() => allEmployees.filter(emp => emp.role === "Manager"), [allEmployees]);
    const options = useMemo(() => {
        if (step3.manager && !managerEmployees.some(emp => emp.id === step3.manager.id)) {
            return [step3.manager, ...managerEmployees];
        }
        return managerEmployees;
    }, [managerEmployees, step3.manager]);

    return (
        <Stack spacing={2}>
            <Typography variant="h5" align="center">Step 3: Manager</Typography>
            <Autocomplete
                options={options}
                getOptionLabel={(option) => option ? `${option.lastName} ${option.firstName}` : ""}
                value={step3.manager || null}
                isOptionEqualToValue={(option, value) => option.id === value.id}
                onChange={(e, newValue) => {
                    if (readOnly) return;
                    setFormData(prev => ({ ...prev, step3: { ...prev.step3, manager: newValue } }));
                }}
                renderInput={(params) => <TextField {...params} label="Select manager" disabled={readOnly} />}
                disabled={readOnly}
            />
        </Stack>
    );
}