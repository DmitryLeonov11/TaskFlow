import { useState, useEffect } from 'react';
import { NavLink, useNavigate } from 'react-router-dom';
import { LayoutDashboard, Clock, Bell, LogOut, Sun, Moon, Shield, FolderOpen, Plus, Trash2 } from 'lucide-react';
import { useAuthStore } from '../../stores/authStore';
import { useThemeStore } from '../../stores/themeStore';
import { useNotificationStore } from '../../stores/notificationStore';
import { useProjectStore } from '../../stores/projectStore';
import { useTaskStore } from '../../stores/taskStore';
import NotificationsPanel from '../Notification/NotificationsPanel';

export default function Sidebar() {
  const navigate = useNavigate();
  const { user, isAuthenticated, logout } = useAuthStore();
  const { isDark, toggle } = useThemeStore();
  const { unreadCount, fetchNotifications } = useNotificationStore();
  const { projects, selectedProjectId, fetchProjects, createProject, deleteProject, selectProject } = useProjectStore();
  const { fetchTasks } = useTaskStore();

  const [showNotifications, setShowNotifications] = useState(false);
  const [showNewProject, setShowNewProject] = useState(false);
  const [newProjectName, setNewProjectName] = useState('');

  useEffect(() => {
    if (isAuthenticated) {
      fetchNotifications();
      fetchProjects();
    }
  }, [isAuthenticated]);

  const handleLogout = async () => {
    await logout();
    navigate('/auth');
  };

  const handleCreateProject = async () => {
    if (!newProjectName.trim()) return;
    await createProject({ name: newProjectName.trim() });
    setNewProjectName('');
    setShowNewProject(false);
  };

  const handleSelectProject = (id: string | null) => {
    selectProject(id);
    fetchTasks({ projectId: id ?? undefined });
  };

  const initials =
    (user?.firstName?.[0] ?? user?.email?.[0] ?? 'U').toUpperCase() +
    (user?.lastName?.[0] ?? '').toUpperCase();

  const navLinkClass = ({ isActive }: { isActive: boolean }) =>
    `flex items-center gap-2 px-3 py-2 text-sm rounded-lg transition-colors ${
      isActive
        ? 'bg-primary/10 text-primary font-semibold'
        : 'text-base-content/70 hover:bg-base-300 hover:text-base-content'
    }`;

  return (
    <aside className="w-60 flex-shrink-0 flex flex-col pt-5 pb-4 bg-base-200 border-r border-base-300">
      {/* Logo + theme toggle */}
      <div className="flex items-center px-5 mb-6 gap-2.5">
        <div className="w-7 h-7 rounded-lg bg-primary text-primary-content flex items-center justify-center font-bold text-sm shadow-sm">
          T
        </div>
        <span className="text-lg font-bold tracking-tight text-base-content">TaskFlow</span>
        <button
          onClick={toggle}
          className="ml-auto p-1.5 rounded-lg hover:bg-base-300 text-base-content/50 hover:text-base-content transition-colors"
          title={isDark ? 'Switch to light' : 'Switch to dark'}
        >
          {isDark ? <Sun className="w-4 h-4" /> : <Moon className="w-4 h-4" />}
        </button>
      </div>

      {/* Nav */}
      <nav className="px-3 space-y-0.5">
        <NavLink to="/board" className={navLinkClass}>
          <LayoutDashboard className="w-4 h-4" /> Board
        </NavLink>
        <NavLink to="/today" className={navLinkClass}>
          <Clock className="w-4 h-4" /> Today
        </NavLink>
      </nav>

      {/* Projects */}
      <div className="px-3 mt-4">
        <div className="flex items-center justify-between mb-1">
          <span className="text-xs font-semibold text-base-content/50 uppercase tracking-wider px-1">Projects</span>
          <button
            onClick={() => setShowNewProject(!showNewProject)}
            className="p-1 rounded hover:bg-base-300 text-base-content/50 hover:text-base-content transition-colors"
            title="New project"
          >
            <Plus className="w-3.5 h-3.5" />
          </button>
        </div>

        {showNewProject && (
          <div className="flex gap-1 mb-1">
            <input
              autoFocus
              value={newProjectName}
              onChange={(e) => setNewProjectName(e.target.value)}
              onKeyDown={(e) => { if (e.key === 'Enter') handleCreateProject(); if (e.key === 'Escape') setShowNewProject(false); }}
              placeholder="Project name"
              className="flex-1 px-2 py-1 text-xs border border-base-300 rounded bg-base-100 text-base-content focus:ring-1 focus:ring-primary focus:outline-none"
            />
            <button onClick={handleCreateProject} className="px-2 py-1 text-xs bg-primary text-primary-content rounded hover:bg-primary/90">
              Add
            </button>
          </div>
        )}

        <button
          onClick={() => handleSelectProject(null)}
          className={`w-full flex items-center gap-2 px-2 py-1.5 text-xs rounded transition-colors ${
            !selectedProjectId ? 'bg-primary/10 text-primary font-semibold' : 'text-base-content/60 hover:bg-base-300 hover:text-base-content'
          }`}
        >
          <FolderOpen className="w-3.5 h-3.5" /> All tasks
        </button>

        {projects.map((p) => (
          <div key={p.id} className="group flex items-center">
            <button
              onClick={() => handleSelectProject(p.id)}
              className={`flex-1 flex items-center gap-2 px-2 py-1.5 text-xs rounded transition-colors ${
                selectedProjectId === p.id ? 'bg-primary/10 text-primary font-semibold' : 'text-base-content/60 hover:bg-base-300 hover:text-base-content'
              }`}
            >
              <span
                className="w-2 h-2 rounded-full flex-shrink-0"
                style={{ backgroundColor: p.color ?? '#6366f1' }}
              />
              <span className="truncate">{p.name}</span>
              <span className="ml-auto text-[10px] opacity-60">{p.taskCount}</span>
            </button>
            <button
              onClick={() => deleteProject(p.id)}
              className="opacity-0 group-hover:opacity-100 p-1 rounded hover:bg-error/10 text-error transition-opacity"
            >
              <Trash2 className="w-3 h-3" />
            </button>
          </div>
        ))}
      </div>

      {/* Bottom section */}
      <div className="px-3 mt-auto space-y-2">
        {/* Notifications */}
        <div className="relative">
          <button
            onClick={() => setShowNotifications(!showNotifications)}
            className="w-full flex items-center justify-between px-3 py-2 rounded-lg border border-base-300 hover:bg-base-300 transition-colors text-sm"
          >
            <div className="flex items-center gap-2">
              <span className="relative inline-flex">
                <Bell className="w-4 h-4 text-primary" />
                {unreadCount > 0 && (
                  <span className="absolute -top-1 -right-1 inline-flex items-center justify-center min-w-[16px] h-4 px-1 rounded-full bg-error text-error-content text-[10px] font-semibold">
                    {unreadCount > 9 ? '9+' : unreadCount}
                  </span>
                )}
              </span>
              <span className="text-base-content/80 font-medium">Notifications</span>
            </div>
          </button>
          {showNotifications && <NotificationsPanel onClose={() => setShowNotifications(false)} />}
        </div>

        {/* User */}
        {isAuthenticated ? (
          <div className="space-y-1.5">
            <div className="flex items-center gap-2.5 px-3 py-2.5 rounded-lg bg-base-100 border border-base-300">
              <div className="w-7 h-7 rounded-full bg-gradient-to-tr from-blue-500 to-purple-500 flex items-center justify-center text-white font-medium text-xs flex-shrink-0">
                {initials}
              </div>
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-base-content truncate">
                  {user?.firstName || user?.email?.split('@')[0] || 'User'}
                </p>
                <p className="text-xs text-base-content/50 truncate">{user?.email}</p>
              </div>
            </div>
            <button
              onClick={handleLogout}
              className="w-full flex items-center justify-center gap-1.5 px-3 py-1.5 rounded-lg border border-base-300 text-xs text-base-content/70 hover:bg-base-300 transition-colors"
            >
              <LogOut className="w-3.5 h-3.5" /> Logout
            </button>
          </div>
        ) : (
          <NavLink
            to="/auth"
            className="flex items-center gap-2.5 px-3 py-2.5 rounded-lg hover:bg-base-300 cursor-pointer transition-colors"
          >
            <Shield className="w-4 h-4 text-base-content/60" />
            <span className="text-sm font-medium text-base-content/80">Login</span>
          </NavLink>
        )}
      </div>
    </aside>
  );
}
