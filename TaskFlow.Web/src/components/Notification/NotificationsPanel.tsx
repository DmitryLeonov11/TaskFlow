import { useEffect, useRef } from 'react';
import { X, CheckCheck, Bell } from 'lucide-react';
import { useNotificationStore } from '../../stores/notificationStore';
import dayjs from 'dayjs';
import relativeTime from 'dayjs/plugin/relativeTime';

dayjs.extend(relativeTime);

interface Props { onClose: () => void; }

export default function NotificationsPanel({ onClose }: Props) {
  const { notifications, markAsRead, markAllAsRead } = useNotificationStore();
  const ref = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handler = (e: MouseEvent) => {
      if (ref.current && !ref.current.contains(e.target as Node)) onClose();
    };
    document.addEventListener('mousedown', handler);
    return () => document.removeEventListener('mousedown', handler);
  }, [onClose]);

  return (
    <div
      ref={ref}
      className="absolute bottom-full left-0 mb-2 w-80 bg-base-100 border border-base-300 rounded-xl shadow-xl z-50"
    >
      <div className="flex items-center justify-between px-4 py-3 border-b border-base-300">
        <span className="font-semibold text-sm text-base-content">Notifications</span>
        <div className="flex items-center gap-1">
          <button
            onClick={markAllAsRead}
            className="p-1.5 rounded hover:bg-base-200 text-base-content/60 hover:text-base-content transition-colors"
            title="Mark all as read"
          >
            <CheckCheck className="w-4 h-4" />
          </button>
          <button
            onClick={onClose}
            className="p-1.5 rounded hover:bg-base-200 text-base-content/60 hover:text-base-content transition-colors"
          >
            <X className="w-4 h-4" />
          </button>
        </div>
      </div>

      <div className="max-h-80 overflow-y-auto">
        {notifications.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-8 text-base-content/40">
            <Bell className="w-8 h-8 mb-2" />
            <p className="text-sm">No notifications</p>
          </div>
        ) : (
          notifications.map((n) => (
            <button
              key={n.id}
              onClick={() => !n.isRead && markAsRead(n.id)}
              className={`w-full text-left px-4 py-3 border-b border-base-200 hover:bg-base-200 transition-colors last:border-0 ${
                n.isRead ? 'opacity-60' : ''
              }`}
            >
              <div className="flex items-start gap-2">
                {!n.isRead && <span className="w-2 h-2 rounded-full bg-primary mt-1.5 flex-shrink-0" />}
                <div className={n.isRead ? 'ml-4' : ''}>
                  <p className="text-xs text-base-content leading-relaxed">{n.message}</p>
                  <p className="text-[10px] text-base-content/40 mt-0.5">{dayjs(n.createdAt).fromNow()}</p>
                </div>
              </div>
            </button>
          ))
        )}
      </div>
    </div>
  );
}
