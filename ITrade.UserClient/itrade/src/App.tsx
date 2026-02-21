import { type ReactNode } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { UserProvider, useUser } from './context';
import { Navbar } from './components/navbar';
import LoginPage from './pages/LoginPage';
import RegisterPage from './pages/RegisterPage';
import DashboardPage from './pages/DashboardPage';
import ProjectDetailPage from './pages/ProjectDetailPage';
import ProfilePage from './pages/ProfilePage';
import RequestsPage from './pages/RequestsPage';
import MyProjectsPage from './pages/MyProjectsPage';
import SettingsPage from './pages/SettingsPage';

/** Layout for authenticated pages — renders the Navbar above the page content */
function ProtectedLayout({ children }: { children: ReactNode }) {
  return (
    <>
      <Navbar />
      <div className="pt-14">{children}</div>
    </>
  );
}

/** Redirects unauthenticated users to /login */
function PrivateRoute({ children }: { children: ReactNode }) {
  const { currentUser, isLoading } = useUser();
  if (isLoading) return null; // or a global spinner
  return currentUser ? (
    <ProtectedLayout>{children}</ProtectedLayout>
  ) : (
    <Navigate to="/login" replace />
  );
}

/** Redirects already-authenticated users away from auth pages */
function GuestRoute({ children }: { children: ReactNode }) {
  const { currentUser, isLoading } = useUser();
  if (isLoading) return null;
  return currentUser ? <Navigate to="/dashboard" replace /> : <>{children}</>;
}

/** Redirects /profile to /users/:currentUserId */
function ProfileRedirect() {
  const { currentUser } = useUser();
  if (!currentUser) return <Navigate to="/login" replace />;
  return <Navigate to={`/users/${currentUser.id}`} replace />;
}

function AppRoutes() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/dashboard" replace />} />
      <Route
        path="/login"
        element={
          <GuestRoute>
            <LoginPage />
          </GuestRoute>
        }
      />
      <Route
        path="/register"
        element={
          <GuestRoute>
            <RegisterPage />
          </GuestRoute>
        }
      />
      <Route
        path="/dashboard"
        element={
          <PrivateRoute>
            <DashboardPage />
          </PrivateRoute>
        }
      />
      <Route
        path="/profile"
        element={
          <PrivateRoute>
            <ProfileRedirect />
          </PrivateRoute>
        }
      />
      <Route
        path="/projects/:projectId"
        element={
          <PrivateRoute>
            <ProjectDetailPage />
          </PrivateRoute>
        }
      />
      <Route
        path="/users/:userId"
        element={
          <PrivateRoute>
            <ProfilePage />
          </PrivateRoute>
        }
      />
      <Route
        path="/requests"
        element={
          <PrivateRoute>
            <RequestsPage />
          </PrivateRoute>
        }
      />
      <Route
        path="/my-projects"
        element={
          <PrivateRoute>
            <MyProjectsPage />
          </PrivateRoute>
        }
      />
      <Route
        path="/settings"
        element={
          <PrivateRoute>
            <SettingsPage />
          </PrivateRoute>
        }
      />
      {/* Catch-all */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}

export default function App() {
  return (
    <BrowserRouter>
      <UserProvider>
        <AppRoutes />
      </UserProvider>
    </BrowserRouter>
  );
}
