const API_BASE = "http://localhost:5224/api";

const getAuthHeader = () => {
    const token = localStorage.getItem("token");
    return token ? { Authorization: `Bearer ${token}` } : {};
};

const handleResponse = async (response) => {
    if (!response.ok) {
        let errorData;
        try {
            errorData = await response.json();
        } catch {
            errorData = {};
        }
        const error = new Error(errorData.message || `Server Error: ${response.status}`);
        error.status = response.status;
        error.response = {
            status: response.status,
            data: errorData,
        };
        throw error;
    }
    return response.status === 204 ? null : response.json();
};

// ---------- Auth ----------
export const register = async (userData) => {
    const response = await fetch(`${API_BASE}/auth/register`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(userData),
    });
    return handleResponse(response);
};

export const login = async (email, password) => {
    const response = await fetch(`${API_BASE}/auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password }),
    });
    if (!response.ok) {
        localStorage.removeItem("token");
    }
    const data = await handleResponse(response);
    if (data.token) {
        localStorage.setItem("token", data.token);
    }
    return data;
};

export const logout = () => {
    localStorage.removeItem("token");
};

export const getCurrentUser = async () => {
    const response = await fetch(`${API_BASE}/auth/me`, {
        headers: getAuthHeader(),
    });
    return handleResponse(response);
};

// ---------- Employees ----------
export const fetchEmployees = async (search = "") => {
    const url = search
        ? `${API_BASE}/employees?searchTerm=${search}`
        : `${API_BASE}/employees`;
    const response = await fetch(url, { headers: getAuthHeader() });
    return handleResponse(response);
};

export const createEmployee = async (employee) => {
    const response = await fetch(`${API_BASE}/employees`, {
        method: "POST",
        headers: { "Content-Type": "application/json", ...getAuthHeader() },
        body: JSON.stringify(employee),
    });
    return handleResponse(response);
};

export const updateEmployee = async (employee) => {
    const response = await fetch(`${API_BASE}/employees/${employee.id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json", ...getAuthHeader() },
        body: JSON.stringify(employee),
    });
    return handleResponse(response);
};

export const deleteEmployee = async (id) => {
    const response = await fetch(`${API_BASE}/employees/${id}`, {
        method: "DELETE",
        headers: getAuthHeader(),
    });
    await handleResponse(response);
    return true;
};

export const checkEmployeeEmailExists = async (email, excludeId = null) => {
    const params = new URLSearchParams();
    params.append("email", email);
    if (excludeId) params.append("excludeId", excludeId);
    const response = await fetch(`${API_BASE}/employees/exists?${params.toString()}`, {
        headers: getAuthHeader(),
    });
    const data = await handleResponse(response);
    return data.exists;
};

// ---------- Projects ----------
export const fetchProjects = async (filters = {}) => {
    const { search, startDateFrom, startDateTo, priorityFrom, priorityTo, sortBy } = filters;
    const params = new URLSearchParams();
    if (search) params.append("search", search);
    if (startDateFrom) params.append("startDateFrom", startDateFrom);
    if (startDateTo) params.append("startDateTo", startDateTo);
    if (priorityFrom) params.append("priorityFrom", priorityFrom);
    if (priorityTo) params.append("priorityTo", priorityTo);
    if (sortBy) params.append("sortBy", sortBy);

    const response = await fetch(`${API_BASE}/projects?${params.toString()}`, {
        headers: getAuthHeader(),
    });
    return handleResponse(response);
};

export const fetchProjectById = async (id) => {
    const response = await fetch(`${API_BASE}/projects/${id}`, {
        headers: getAuthHeader(),
    });
    return handleResponse(response);
};

export const fetchProjectEmployees = async (projectId) => {
    const response = await fetch(`${API_BASE}/projects/${projectId}/employees`, {
        headers: getAuthHeader(),
    });
    return handleResponse(response);
};

export const createProject = async (wizardData) => {
    const payload = {
        projectName: wizardData.step1.projectName,
        customerCompanyName: wizardData.step2.clientCompanyName,
        executorCompanyName: wizardData.step2.executorCompanyName,
        projectManagerId: wizardData.step3.manager.id,
        projectStart: wizardData.step1.startDate.format("YYYY-MM-DD"),
        projectEnd: wizardData.step1.endDate.format("YYYY-MM-DD"),
        priority: Number(wizardData.step1.priority),
        employeeIds: wizardData.step4.employees.map((e) => e.id),
    };
    const response = await fetch(`${API_BASE}/projects`, {
        method: "POST",
        headers: { "Content-Type": "application/json", ...getAuthHeader() },
        body: JSON.stringify(payload),
    });
    return handleResponse(response);
};

export const updateProject = async (id, wizardData, updateTeamOnly = false) => {
    const payload = {
        id: id,
        projectName: wizardData.step1.projectName,
        customerCompanyName: wizardData.step2.clientCompanyName,
        executorCompanyName: wizardData.step2.executorCompanyName,
        projectManagerId: wizardData.step3.manager.id,
        projectStart: wizardData.step1.startDate.toISOString(),
        projectEnd: wizardData.step1.endDate.toISOString(),
        priority: Number(wizardData.step1.priority),
        employeeIds: wizardData.step4.employees.map((e) => e.id),
        updateTeamOnly: updateTeamOnly,
    };
    const response = await fetch(`${API_BASE}/projects/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json", ...getAuthHeader() },
        body: JSON.stringify(payload),
    });
    return handleResponse(response);
};

export const checkProjectNameExists = async (name, excludeId = null) => {
    const params = new URLSearchParams();
    params.append("name", name);
    if (excludeId) params.append("excludeId", excludeId);
    const response = await fetch(`${API_BASE}/projects/exists?${params.toString()}`, {
        headers: getAuthHeader(),
    });
    const data = await handleResponse(response);
    return data.exists;
};

export const deleteProject = async (id) => {
    const response = await fetch(`${API_BASE}/projects/${id}`, {
        method: "DELETE",
        headers: getAuthHeader(),
    });
    await handleResponse(response);
    return true;
};

// ---------- Documents ----------
export const getProjectDocuments = async (projectId) => {
    const token = localStorage.getItem("token");
    const response = await fetch(`${API_BASE}/Documents/project/${projectId}`, {
        headers: {
            Authorization: `Bearer ${token}`,
        },
    });

    if (!response.ok) {
        throw new Error("Failed to fetch documents");
    }

    return await response.json();
};
export const uploadDocument = async (projectId, formData) => {
    const response = await fetch(`${API_BASE}/documents/upload/${projectId}`, {
        method: "POST",
        headers: getAuthHeader(),
        body: formData,
    });
    return handleResponse(response);
};

export const deleteDocument = async (documentId) => {
    const response = await fetch(`${API_BASE}/documents/${documentId}`, {
        method: "DELETE",
        headers: getAuthHeader(),
    });
    await handleResponse(response);
    return true;
};

export const downloadDocument = async (documentId) => {
    const token = localStorage.getItem("token");
    const response = await fetch(`${API_BASE}/Documents/download/${documentId}`, {
        headers: {
            Authorization: `Bearer ${token}`,
        },
    });

    if (!response.ok) {
        throw new Error("Failed to download document");
    }

    return await response.blob();
};

// ---------- Tasks ----------
export const fetchTasks = async (filters = {}) => {
    const { projectId, status, sort } = filters;
    const params = new URLSearchParams();
    if (projectId) params.append("projectId", projectId);
    if (status !== undefined && status !== null && status !== "") {
        params.append("status", status);
    }
    if (sort) params.append("sort", sort);

    const response = await fetch(`${API_BASE}/tasks?${params.toString()}`, {
        headers: getAuthHeader(),
    });
    return handleResponse(response);
};

export const createTask = async (taskData) => {
    const payload = {
        title: taskData.title,
        comment: taskData.comment || "",
        priority: parseInt(taskData.priority) || 0,
        projectId: parseInt(taskData.projectId),
        authorId: parseInt(taskData.authorId),
        executorId: taskData.executorId ? parseInt(taskData.executorId) : null,
    };
    const response = await fetch(`${API_BASE}/tasks`, {
        method: "POST",
        headers: { "Content-Type": "application/json", ...getAuthHeader() },
        body: JSON.stringify(payload),
    });
    return handleResponse(response);
};

export const updateTask = async (taskData) => {
    const payload = {
        id: parseInt(taskData.id),
        title: taskData.title,
        comment: taskData.comment || "",
        priority: parseInt(taskData.priority) || 0,
        status: parseInt(taskData.status),
        executorId: taskData.executorId ? parseInt(taskData.executorId) : null,
    };
    const response = await fetch(`${API_BASE}/tasks/${taskData.id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json", ...getAuthHeader() },
        body: JSON.stringify(payload),
    });
    return handleResponse(response);
};

export const deleteTask = async (id) => {
    const response = await fetch(`${API_BASE}/tasks/${id}`, {
        method: "DELETE",
        headers: getAuthHeader(),
    });
    await handleResponse(response);
    return true;
};

export const checkTaskTitleExists = async (title, projectId, excludeId = null) => {
    const params = new URLSearchParams();
    params.append("title", title);
    params.append("projectId", projectId);
    if (excludeId) params.append("excludeId", excludeId);
    const response = await fetch(`${API_BASE}/tasks/exists?${params.toString()}`, {
        headers: getAuthHeader(),
    });
    const data = await handleResponse(response);
    return data.exists;
};