import { createContext, useState, useContext, useEffect, useCallback, useMemo } from "react";
import { login, logout, getCurrentUser } from "../api/api";

export const AuthContext = createContext(null);

export const AuthProvider = ({ children }) => {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const checkAuth = async () => {
            const token = localStorage.getItem("token");
            if (token) {
                try {
                    const userData = await getCurrentUser();
                    setUser(userData);
                } catch (e) {
                    localStorage.removeItem("token");
                }
            }
            setLoading(false);
        };
        checkAuth();
    }, []);

    const loginAction = useCallback(async (email, password) => {
        const data = await login(email, password);
        setUser(data.user);
        return data;
    }, []);

    const logoutAction = useCallback(async () => {
        try {
            await logout();
        } finally {
            localStorage.removeItem("token");
            setUser(null);
        }
    }, []);

    const hasRole = useCallback((roleName) => {
        return user?.roles?.includes(roleName);
    }, [user]);

    const contextValue = useMemo(() => ({
        user,
        loginAction,
        logout: logoutAction,
        loading,
        hasRole
    }), [user, loginAction, logoutAction, loading, hasRole]);

    return (
        <AuthContext.Provider value={contextValue}>
            {!loading && children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => {
    const ctx = useContext(AuthContext);
    if (!ctx) throw new Error("useAuth must be used inside AuthProvider");
    return ctx;
};