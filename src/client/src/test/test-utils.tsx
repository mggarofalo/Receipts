import { render, type RenderOptions } from "@testing-library/react";
import { MemoryRouter } from "react-router";
import type { ReactNode } from "react";
import { AuthContext, type AuthContextValue } from "@/contexts/auth-context";

const defaultAuth: AuthContextValue = {
  user: null,
  isLoading: false,
  mustResetPassword: false,
  login: async () => {},
  logout: async () => {},
  changePassword: async () => {},
};

interface WrapperOptions {
  auth?: Partial<AuthContextValue>;
  route?: string;
}

export function createWrapper({ auth, route = "/" }: WrapperOptions = {}) {
  const authValue = { ...defaultAuth, ...auth };
  return function Wrapper({ children }: { children: ReactNode }) {
    return (
      <MemoryRouter initialEntries={[route]}>
        <AuthContext.Provider value={authValue}>
          {children}
        </AuthContext.Provider>
      </MemoryRouter>
    );
  };
}

export function renderWithProviders(
  ui: React.ReactElement,
  options?: WrapperOptions & Omit<RenderOptions, "wrapper">,
) {
  const { auth, route, ...renderOptions } = options ?? {};
  return render(ui, {
    wrapper: createWrapper({ auth, route }),
    ...renderOptions,
  });
}
