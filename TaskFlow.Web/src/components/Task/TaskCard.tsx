import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { Flag, Calendar, Tag as TagIcon, CheckSquare, MessageSquare } from 'lucide-react';
import type { Task } from '../../types';
import { PRIORITY_COLORS, PRIORITY_LABELS, TaskStatus } from '../../constants/task';
import dayjs from 'dayjs';

interface Props {
  task: Task;
  onClick: (task: Task) => void;
}

const cardClass =
  'bg-base-100 rounded-xl border border-base-300 p-3 shadow-sm hover:shadow-md hover:border-primary/30 transition-all cursor-pointer group select-none';

function TaskCardBody({ task }: { task: Task }) {
  const doneSubtasks = task.subtasks.filter((s) => s.status === TaskStatus.Done).length;
  const totalSubtasks = task.subtasks.length;
  const isOverdue =
    task.deadline && dayjs(task.deadline).isBefore(dayjs()) && task.status !== TaskStatus.Done;

  return (
    <>
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

      <p className="text-sm font-medium text-base-content leading-snug mb-2 line-clamp-2">
        {task.title}
      </p>

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
              style={{ width: `${(doneSubtasks / totalSubtasks) * 100}%` }}
            />
          </div>
        </div>
      )}

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
          <MessageSquare className="w-3 h-3" />
          {task.comments.length}
        </div>
      )}
    </>
  );
}

export function TaskCardPreview({ task }: { task: Task }) {
  return (
    <div className={cardClass}>
      <TaskCardBody task={task} />
    </div>
  );
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

  return (
    <div
      ref={setNodeRef}
      style={style}
      {...attributes}
      {...listeners}
      onClick={() => onClick(task)}
      className={cardClass}
    >
      <TaskCardBody task={task} />
    </div>
  );
}
