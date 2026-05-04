import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuthStore } from '../stores/authStore';
import api from '../api/client';

type Mode = 'login' | 'register' | 'reset';

export default function AuthPage() {
  const navigate = useNavigate();
  const { login, register } = useAuthStore();

  const [mode, setMode] = useState<Mode>('login');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [resetEmail, setResetEmail] = useState('');
  const [resetToken, setResetToken] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');
  const [loading, setLoading] = useState(false);

  const handleLogin = async () => {
    setError(''); setMessage(''); setLoading(true);
    try {
      await login({ email, password });
      navigate('/board');
    } catch (e: any) {
      setError(e.response?.data?.message || 'Login failed.');
    } finally { setLoading(false); }
  };

  const handleRegister = async () => {
    setError(''); setMessage(''); setLoading(true);
    if (password !== confirmPassword) { setError('Passwords do not match.'); setLoading(false); return; }
    try {
      await register({ email, password, confirmPassword, firstName, lastName });
      setMessage('Registration successful! Redirecting...');
      const t = setTimeout(() => { navigate('/auth'); setMode('login'); }, 1000);
      return () => clearTimeout(t);
    } catch (e: any) {
      setError(e.response?.data?.message || 'Registration failed.');
    } finally { setLoading(false); }
  };

  const handleRequestReset = async () => {
    setError(''); setMessage(''); setLoading(true);
    try {
      await api.post('/auth/request-password-reset', { email: resetEmail });
      setMessage('If the email exists, instructions were sent (token in server logs).');
    } catch (e: any) {
      setError(e.response?.data?.message || 'Failed to request reset.');
    } finally { setLoading(false); }
  };

  const handleResetPassword = async () => {
    setError(''); setMessage(''); setLoading(true);
    try {
      await api.post('/auth/reset-password', { email: resetEmail, token: resetToken, newPassword });
      setMessage('Password reset! You can now login.');
      setMode('login');
    } catch (e: any) {
      setError(e.response?.data?.message || 'Failed to reset password.');
    } finally { setLoading(false); }
  };

  const inputClass = 'w-full border border-base-300 rounded-lg px-3 py-2 text-sm bg-base-200 text-base-content placeholder:text-base-content/40 focus:ring-2 focus:ring-primary focus:border-primary focus:outline-none';
  const btnPrimary = `w-full inline-flex justify-center items-center px-4 py-2.5 rounded-lg bg-primary text-primary-content text-sm font-medium hover:bg-primary/90 transition-colors shadow-sm ${loading ? 'opacity-60 pointer-events-none' : ''}`;

  return (
    <div className="min-h-screen flex items-center justify-center bg-base-200 px-4">
      <div className="w-full max-w-md bg-base-100 rounded-2xl shadow-lg border border-base-200 p-8 space-y-6">
        <div>
          <h1 className="text-2xl font-bold text-base-content text-center">
            {mode === 'login' ? 'Login' : mode === 'register' ? 'Create Account' : 'Reset Password'}
          </h1>
          <p className="text-sm text-base-content/60 text-center mt-1">
            {mode === 'login' ? 'Sign in to TaskFlow.' : mode === 'register' ? 'Create your account.' : 'Reset your password.'}
          </p>
        </div>

        {error && <div className="text-sm text-red-600 bg-red-50 border border-red-100 px-3 py-2 rounded-lg">{error}</div>}
        {message && <div className="text-sm text-emerald-700 bg-emerald-50 border border-emerald-100 px-3 py-2 rounded-lg">{message}</div>}

        {/* Login */}
        {mode === 'login' && (
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-base-content/80 mb-1">Email</label>
              <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} className={inputClass} placeholder="you@example.com" />
            </div>
            <div>
              <label className="block text-sm font-medium text-base-content/80 mb-1">Password</label>
              <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} onKeyDown={(e) => e.key === 'Enter' && handleLogin()} className={inputClass} placeholder="••••••••" />
            </div>
            <button onClick={handleLogin} className={btnPrimary}>{loading ? 'Logging in...' : 'Login'}</button>
            <div className="flex items-center justify-between">
              <button onClick={() => setMode('register')} className="text-xs text-base-content/60 hover:text-base-content/80">No account? Register</button>
              <button onClick={() => setMode('reset')} className="text-xs text-base-content/60 hover:text-base-content/80">Forgot password?</button>
            </div>
          </div>
        )}

        {/* Register */}
        {mode === 'register' && (
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-sm font-medium text-base-content/80 mb-1">First Name</label>
                <input value={firstName} onChange={(e) => setFirstName(e.target.value)} className={inputClass} placeholder="John" />
              </div>
              <div>
                <label className="block text-sm font-medium text-base-content/80 mb-1">Last Name</label>
                <input value={lastName} onChange={(e) => setLastName(e.target.value)} className={inputClass} placeholder="Doe" />
              </div>
            </div>
            <div>
              <label className="block text-sm font-medium text-base-content/80 mb-1">Email</label>
              <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} className={inputClass} placeholder="you@example.com" />
            </div>
            <div>
              <label className="block text-sm font-medium text-base-content/80 mb-1">Password</label>
              <input type="password" value={password} onChange={(e) => setPassword(e.target.value)} className={inputClass} placeholder="Min 8 chars" minLength={8} />
            </div>
            <div>
              <label className="block text-sm font-medium text-base-content/80 mb-1">Confirm Password</label>
              <input type="password" value={confirmPassword} onChange={(e) => setConfirmPassword(e.target.value)} className={inputClass} placeholder="••••••••" />
            </div>
            <button onClick={handleRegister} className={btnPrimary}>{loading ? 'Registering...' : 'Register'}</button>
            <button onClick={() => setMode('login')} className="w-full text-xs text-base-content/60 hover:text-base-content/80 text-center">Already have an account? Login</button>
          </div>
        )}

        {/* Reset */}
        {mode === 'reset' && (
          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-base-content/80 mb-1">Email</label>
              <input type="email" value={resetEmail} onChange={(e) => setResetEmail(e.target.value)} className={inputClass} placeholder="you@example.com" />
            </div>
            <button onClick={handleRequestReset} className={`${btnPrimary} bg-neutral text-neutral-content hover:bg-neutral/90`}>
              {loading ? 'Sending...' : 'Request reset link'}
            </button>
            <div>
              <label className="block text-sm font-medium text-base-content/80 mb-1">Reset Token</label>
              <textarea value={resetToken} onChange={(e) => setResetToken(e.target.value)} rows={2} className={`${inputClass} resize-none`} placeholder="Paste token from server logs" />
            </div>
            <div>
              <label className="block text-sm font-medium text-base-content/80 mb-1">New Password</label>
              <input type="password" value={newPassword} onChange={(e) => setNewPassword(e.target.value)} className={inputClass} placeholder="New secure password" />
            </div>
            <div className="flex gap-3">
              <button onClick={() => setMode('login')} className="flex-1 px-4 py-2.5 rounded-lg border border-base-300 text-sm font-medium text-base-content/80 hover:bg-base-200">Back</button>
              <button onClick={handleResetPassword} className={`flex-1 ${btnPrimary}`}>{loading ? 'Resetting...' : 'Reset password'}</button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
