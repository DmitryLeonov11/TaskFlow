import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { useEffect } from 'react';
import { useAuthStore } from './stores/authStore';
import Sidebar from './components/Layout/Sidebar';
import AuthPage from './pages/AuthPage';
import BoardPage from './pages/BoardPage';
import TodayPage from './pages/TodayPage';
import signalrService from './services/signalrService';
import notificationsSignalrService from './services/notificationsSignalrService';

function RequireAuth({ children }: { children: React.ReactNode }) {
  const token = localStorage.getItem('auth_token');
  if (!token) return <Navigate to="/auth" replace />;
  return <>{children}</>;
}

function App() {
  const { restoreFromStorage, isAuthenticated } = useAuthStore();

  useEffect(() => {
    restoreFromStorage();
  }, [restoreFromStorage]);

  useEffect(() => {
    if (isAuthenticated) {
      signalrService.startConnection();
      notificationsSignalrService.startConnection();
    }
  }, [isAuthenticated]);

  return (
    <BrowserRouter>
      {/* Desktop layout */}
      <div className="h-screen sm:flex hidden bg-base-100 overflow-hidden font-sans text-base-content">
        <Sidebar />
        <main className="flex-1 flex flex-col relative focus:outline-none min-w-0 overflow-hidden">
          <Routes>
            <Route path="/" element={<Navigate to="/board" replace />} />
            <Route path="/auth" element={<AuthPage />} />
            <Route
              path="/board"
              element={
                <RequireAuth>
                  <BoardPage />
                </RequireAuth>
              }
            />
            <Route
              path="/today"
              element={
                <RequireAuth>
                  <TodayPage />
                </RequireAuth>
              }
            />
            <Route path="*" element={<Navigate to="/board" replace />} />
          </Routes>
        </main>
      </div>

      {/* Mobile fallback */}
      <div className="flex sm:hidden h-screen items-center justify-center p-8 text-center bg-base-200 flex-col">
        <div className="w-12 h-12 rounded-xl bg-primary text-primary-content flex items-center justify-center font-bold text-xl mb-4">
          T
        </div>
        <h2 className="text-xl font-bold mb-2 text-base-content">Desktop recommended</h2>
        <p className="text-base-content/60 text-sm">TaskFlow works best on larger screens.</p>
      </div>
    </BrowserRouter>
  );
}

export default App;
