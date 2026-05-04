import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { Flag, Calendar, Tag as TagIcon, CheckSquare, Square } from 'lucide-react';
import type { Task } from '../../types';
import dayjs from 'dayjs';

const PRIORITY_COLORS: Record<number, string> = {
  0: 'text-base-content/40',
  1: 'text-info',
  2: 'text-warning',
  3: 'text-error',
};

const PRIORITY_LABELS: Record<number, string> = { 0: 'Low', 1: 'Medium', 2: 'High', 3: 'Urgent' };

interface Props {
  task: Task;
  onClick: (task: Task) => void;
}

export default function TaskCard({ task, onClick }: Props) {
  const { attributes, listeners, setNodeRef, transform, transition, isDragging } = useSortable({
    id: task.id,
  });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.4 : 1,
  };

  const doneSubtasks = task.subtasks.filter((s) => s.status === 3).length;
  const totalSubtasks = task.subtasks.length;
  const isOverdue = task.deadline && dayjs(task.deadline).isBefore(dayjs()) && task.status !== 3;

  return (
    <div
      ref={setNodeRef}
      style={style}
      {...attributes}
      {...listeners}
      onClick={() => onClick(task)}
      className="bg-base-100 rounded-xl border border-base-300 p-3 shadow-sm hover:shadow-md hover:border-primary/30 transition-all cursor-pointer group select-none"
    >
      {/* Tags */}
      {task.tags?.length > 0 && (
        <div className="flex flex-wrap gap-1 mb-2">
          {task.tags.map((tag) => (
            <span
              key={tag.id}
              className="inline-flex items-center gap-0.5 px-1.5 py-0.5 rounded-full text-[10px] font-medium"
              style={{ backgroundColor: tag.color ? `${tag.color}25` : '#6366f125', color: tag.color ?? '#6366f1' }}
            >
              <TagIcon className="w-2.5 h-2.5" />
              {tag.name}
            </span>
          ))}
        </div>
      )}

      {/* Title */}
      <p className="text-sm font-medium text-base-content leading-snug mb-2 line-clamp-2">
        {task.title}
      </p>

      {/* Subtasks progress */}
      {totalSubtasks > 0 && (
        <div className="mb-2">
          <div className="flex items-center justify-between text-xs text-base-content/50 mb-0.5">
            <span className="flex items-center gap-1">
              <CheckSquare className="w-3 h-3" />
              {doneSubtasks}/{totalSubtasks}
            </span>
          </div>
          <div className="h-1 bg-base-300 rounded-full overflow-hidden">
            <div
              className="h-full bg-success rounded-full transition-all"
              style={{ width: `${totalSubtasks ? (doneSubtasks / totalSubtasks) * 100 : 0}%` }}
            />
          </div>
        </div>
      )}

      {/* Footer */}
      <div className="flex items-center justify-between mt-1">
        <div className="flex items-center gap-1">
          <Flag className={`w-3.5 h-3.5 ${PRIORITY_COLORS[task.priority]}`} />
          <span className="text-[10px] text-base-content/50">{PRIORITY_LABELS[task.priority]}</span>
        </div>

        {task.deadline && (
          <span className={`flex items-center gap-0.5 text-[10px] ${isOverdue ? 'text-error' : 'text-base-content/40'}`}>
            <Calendar className="w-3 h-3" />
            {dayjs(task.deadline).format('MMM D')}
          </span>
        )}
      </div>

      {task.comments?.length > 0 && (
        <div className="mt-1.5 flex items-center gap-1 text-[10px] text-base-content/40">
          <svg className="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
          </svg>
          {task.comments.length}
        </div>
      )}
    </div>
  );
}
