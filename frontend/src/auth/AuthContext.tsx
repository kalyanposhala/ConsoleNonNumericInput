import { createContext, useContext, useMemo, useState, type ReactNode } from "react";
import type { UserRole } from "../types/models";

interface AuthUser {
  memberId: string;
  fullName: string;
  role: UserRole;
}

interface AuthContextValue {
  user: AuthUser | null;
  login: (user: AuthUser, token: string) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

// Placeholder only: no real login flow is wired up yet. This exists so the
// route shell (ProtectedRoute, role-based redirects) has something to sit on
// while the real auth API is designed.
export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      login: (u, token) => {
        localStorage.setItem("kmss_token", token);
        setUser(u);
      },
      logout: () => {
        localStorage.removeItem("kmss_token");
        setUser(null);
      },
    }),
    [user],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
