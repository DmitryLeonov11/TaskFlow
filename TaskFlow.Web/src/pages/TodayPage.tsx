import { useEffect, useMemo, useState } from 'react';
import { Calendar, Flag, CheckCircle2, Circle } from 'lucide-react';
import { useTaskStore } from '../stores/taskStore';
import { PRIORITY_COLORS, PRIORITY_LABELS, TaskStatus } from '../constants/task';
import dayjs from 'dayjs';
import type { Task, UpdateTaskInput } from '../types';
import TaskForm from '../components/Task/TaskForm';

export default function TodayPage() {
  const { tasks, loading, fetchTasks, updateTask, deleteTask } = useTaskStore();
  const [editingTask, setEditingTask] = useState<Task | null>(null);

  useEffect(() => { fetchTasks(); }, []);

  const { overdueTasks, dueTodayTasks } = useMemo(() => {
    const startOfToday = dayjs().startOf('day');
    const startOfTomorrow = startOfToday.add(1, 'day');
    const overdue: Task[] = [];
    const dueToday: Task[] = [];

    for (const t of tasks) {
      if (t.status === TaskStatus.Done || !t.deadline) continue;
      const d = dayjs(t.deadline);
      if (d.isBefore(startOfToday)) overdue.push(t);
      else if (d.isBefore(startOfTomorrow)) dueToday.push(t);
    }

    const byDeadline = (a: Task, b: Task) =>
      dayjs(a.deadline!).valueOf() - dayjs(b.deadline!).valueOf();

    return {
      overdueTasks: overdue.sort(byDeadline),
      dueTodayTasks: dueToday.sort(byDeadline),
    };
  }, [tasks]);

  const totalCount = overdueTasks.length + dueTodayTasks.length;

  const handleToggleDone = async (task: Task) => {
    await updateTask(task.id, { status: task.status === TaskStatus.Done ? TaskStatus.Todo : TaskStatus.Done });
  };

  const handleSave = async (data: UpdateTaskInput) => {
    if (!editingTask) return;
    await updateTask(editingTask.id, data);
    setEditingTask(null);
  };

  const handleDelete = async (id: string) => {
    await deleteTask(id);
    setEditingTask(null);
  };

  const TaskRow = ({ task, isOverdue }: { task: Task; isOverdue: boolean }) => (
    <div className="flex items-center gap-3 p-3 bg-base-100 rounded-xl border border-base-300 hover:border-primary/30 transition-colors group">
      <button onClick={() => handleToggleDone(task)} className="flex-shrink-0">
        {task.status === TaskStatus.Done
          ? <CheckCircle2 className="w-5 h-5 text-success" />
          : <Circle className="w-5 h-5 text-base-content/30" />
        }
      </button>
      <div
        className="flex-1 min-w-0 cursor-pointer"
        onClick={() => setEditingTask(task)}
      >
        <p className={`text-sm font-medium truncate ${task.status === TaskStatus.Done ? 'line-through text-base-content/40' : 'text-base-content'}`}>
          {task.title}
        </p>
        {task.deadline && (
          <p className={`text-xs flex items-center gap-1 mt-0.5 ${isOverdue ? 'text-error' : 'text-base-content/50'}`}>
            <Calendar className="w-3 h-3" />
            {dayjs(task.deadline).format('MMM D, YYYY')}
          </p>
        )}
      </div>
      <span title={PRIORITY_LABELS[task.priority]}><Flag className={`w-4 h-4 ${PRIORITY_COLORS[task.priority]}`} /></span>
    </div>
  );

  return (
    <div className="h-full flex flex-col bg-base-200">
      <header className="bg-base-100 border-b border-base-300 px-6 py-4 shadow-sm">
        <h1 className="text-xl font-bold text-base-content">Today</h1>
        <p className="text-sm text-base-content/50 mt-0.5">{dayjs().format('dddd, MMMM D, YYYY')}</p>
      </header>

      <div className="flex-1 overflow-y-auto p-6">
        {loading ? (
          <div className="flex justify-center items-center h-full">
            <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-primary" />
          </div>
        ) : totalCount === 0 ? (
          <div className="flex flex-col items-center justify-center h-full text-base-content/40">
            <CheckCircle2 className="w-12 h-12 mb-3" />
            <p className="text-lg font-medium">All clear for today!</p>
            <p className="text-sm">No tasks due today or overdue.</p>
          </div>
        ) : (
          <div className="max-w-xl mx-auto space-y-6">
            {overdueTasks.length > 0 && (
              <section>
                <h2 className="text-sm font-semibold text-error mb-2 flex items-center gap-1.5">
                  <Calendar className="w-4 h-4" /> Overdue ({overdueTasks.length})
                </h2>
                <div className="space-y-2">
                  {overdueTasks.map((t) => <TaskRow key={t.id} task={t} isOverdue />)}
                </div>
              </section>
            )}

            {dueTodayTasks.length > 0 && (
              <section>
                <h2 className="text-sm font-semibold text-base-content/70 mb-2 flex items-center gap-1.5">
                  <Calendar className="w-4 h-4" /> Due Today ({dueTodayTasks.length})
                </h2>
                <div className="space-y-2">
                  {dueTodayTasks.map((t) => <TaskRow key={t.id} task={t} isOverdue={false} />)}
                </div>
              </section>
            )}
          </div>
        )}
      </div>

      {editingTask && (
        <div
          className="fixed inset-0 bg-black/40 backdrop-blur-sm z-50 flex items-center justify-center p-4"
          onClick={(e) => { if (e.target === e.currentTarget) setEditingTask(null); }}
        >
          <div className="w-full max-w-lg max-h-[90vh] overflow-y-auto">
            <TaskForm
              initialData={editingTask}
              isEditing
              onSave={handleSave}
              onDelete={handleDelete}
              onCancel={() => setEditingTask(null)}
            />
          </div>
        </div>
      )}
    </div>
  );
}
