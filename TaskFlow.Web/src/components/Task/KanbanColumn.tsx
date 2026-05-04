import { useDroppable } from '@dnd-kit/core';
import { SortableContext, verticalListSortingStrategy } from '@dnd-kit/sortable';
import { Plus } from 'lucide-react';
import type { Task } from '../../types';
import TaskCard from './TaskCard';

interface Props {
  id: number;
  title: string;
  tasks: Task[];
  onTaskClick: (task: Task) => void;
  onAddTask: (status: number) => void;
}

const COLUMN_COLORS: Record<number, string> = {
  0: 'bg-base-content/5',
  1: 'bg-info/5',
  2: 'bg-warning/5',
  3: 'bg-success/5',
};

export default function KanbanColumn({ id, title, tasks, onTaskClick, onAddTask }: Props) {
  const { setNodeRef, isOver } = useDroppable({ id: String(id) });

  return (
    <div className="flex-shrink-0 w-72 flex flex-col">
      {/* Header */}
      <div className="flex items-center justify-between mb-3">
        <div className="flex items-center gap-2">
          <span className="text-sm font-semibold text-base-content">{title}</span>
          <span className="inline-flex items-center justify-center min-w-[20px] h-5 px-1.5 rounded-full bg-base-300 text-[10px] font-semibold text-base-content/70">
            {tasks.length}
          </span>
        </div>
        <button
          onClick={() => onAddTask(id)}
          className="p-1 rounded-lg hover:bg-base-300 text-base-content/50 hover:text-base-content transition-colors"
          title="Add task"
        >
          <Plus className="w-4 h-4" />
        </button>
      </div>

      {/* Cards */}
      <div
        ref={setNodeRef}
        className={`flex-1 min-h-[120px] rounded-xl p-2 transition-colors space-y-2 ${COLUMN_COLORS[id]} ${isOver ? 'ring-2 ring-primary/40' : ''}`}
      >
        <SortableContext items={tasks.map((t) => t.id)} strategy={verticalListSortingStrategy}>
          {tasks.map((task) => (
            <TaskCard key={task.id} task={task} onClick={onTaskClick} />
          ))}
        </SortableContext>
      </div>
    </div>
  );
}
