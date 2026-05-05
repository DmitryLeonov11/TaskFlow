import { useState, useEffect, useMemo } from 'react';
import {
  DndContext, DragEndEvent, DragStartEvent,
  PointerSensor, useSensor, useSensors, closestCorners, DragOverlay,
} from '@dnd-kit/core';
import { Plus, Search, X } from 'lucide-react';
import { useTaskStore } from '../stores/taskStore';
import { useTagStore } from '../stores/tagStore';
import { useProjectStore } from '../stores/projectStore';
import KanbanColumn from '../components/Task/KanbanColumn';
import TaskCard, { TaskCardPreview } from '../components/Task/TaskCard';
import TaskForm from '../components/Task/TaskForm';
import { STATUSES, TaskStatus } from '../constants/task';
import type { Task, CreateTaskInput, UpdateTaskInput } from '../types';

const COLUMNS = STATUSES.map((s) => ({ id: s.value, title: s.label }));

export default function BoardPage() {
  const { tasks, loading, fetchTasks, createTask, updateTask, moveTask, deleteTask } = useTaskStore();
  const { fetchTags, tags } = useTagStore();
  const { selectedProjectId } = useProjectStore();

  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingTask, setEditingTask] = useState<Task | undefined>();
  const [defaultStatus, setDefaultStatus] = useState<number>(TaskStatus.Todo);
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedTagIds, setSelectedTagIds] = useState<string[]>([]);
  const [activeTask, setActiveTask] = useState<Task | null>(null);

  const sensors = useSensors(useSensor(PointerSensor, { activationConstraint: { distance: 8 } }));

  useEffect(() => {
    fetchTasks({ projectId: selectedProjectId ?? undefined });
    fetchTags();
  }, [selectedProjectId]);

  const filteredTasks = useMemo(() => {
    let result = tasks;
    const term = searchTerm.trim().toLowerCase();
    if (term) result = result.filter((t) => t.title.toLowerCase().includes(term) || t.description?.toLowerCase().includes(term));
    if (selectedTagIds.length) result = result.filter((t) => selectedTagIds.every((id) => t.tags?.some((tag) => tag.id === id)));
    return result;
  }, [tasks, searchTerm, selectedTagIds]);

  const tasksByStatus = useMemo(
    () =>
      Object.fromEntries(
        COLUMNS.map((col) => [
          col.id,
          filteredTasks.filter((t) => t.status === col.id).sort((a, b) => a.orderIndex - b.orderIndex),
        ])
      ) as Record<number, Task[]>,
    [filteredTasks]
  );

  const handleDragStart = (e: DragStartEvent) => {
    const task = tasks.find((t) => t.id === e.active.id);
    setActiveTask(task ?? null);
  };

  const handleDragEnd = async (e: DragEndEvent) => {
    setActiveTask(null);
    const { active, over } = e;
    if (!over) return;

    const taskId = String(active.id);
    const task = tasks.find((t) => t.id === taskId);
    if (!task) return;

    const overId = String(over.id);
    const newStatus = COLUMNS.find((c) => String(c.id) === overId)
      ? Number(overId)
      : tasks.find((t) => t.id === overId)?.status ?? task.status;

    const columnTasks = tasks.filter((t) => t.status === newStatus && t.id !== taskId).sort((a, b) => a.orderIndex - b.orderIndex);
    const overIndex = columnTasks.findIndex((t) => t.id === overId);
    const newOrderIndex = overIndex >= 0 ? overIndex : columnTasks.length;

    if (task.status !== newStatus || task.orderIndex !== newOrderIndex) {
      await moveTask(taskId, newStatus, newOrderIndex);
    }
  };

  const handleTaskClick = (task: Task) => {
    setEditingTask(task);
    setIsFormOpen(true);
  };

  const handleAddTask = (status: number) => {
    setEditingTask(undefined);
    setDefaultStatus(status);
    setIsFormOpen(true);
  };

  const handleSave = async (data: CreateTaskInput | UpdateTaskInput) => {
    if (editingTask) {
      await updateTask(editingTask.id, data as UpdateTaskInput);
    } else {
      await createTask({ ...(data as CreateTaskInput), status: data.status ?? defaultStatus });
    }
    setIsFormOpen(false);
  };

  const handleDelete = async (id: string) => {
    await deleteTask(id);
    setIsFormOpen(false);
  };

  const toggleTag = (id: string) =>
    setSelectedTagIds((prev) => (prev.includes(id) ? prev.filter((t) => t !== id) : [...prev, id]));

  const hasFilters = searchTerm.trim() || selectedTagIds.length > 0;

  return (
    <div className="h-full flex flex-col bg-base-200">
      {/* Header */}
      <header className="bg-base-100 border-b border-base-300 px-6 py-3 shadow-sm z-10">
        <div className="flex items-center justify-between mb-3">
          <h1 className="text-xl font-bold text-base-content">Board</h1>
          <button
            onClick={() => handleAddTask(TaskStatus.Todo)}
            className="bg-primary hover:bg-primary/90 text-primary-content px-4 py-2 rounded-lg text-sm font-medium transition-colors shadow-sm flex items-center gap-1.5"
          >
            <Plus className="h-4 w-4" /> New Task
          </button>
        </div>

        <div className="flex items-center gap-3 flex-wrap">
          <div className="relative">
            <Search className="absolute left-2.5 top-1/2 -translate-y-1/2 w-4 h-4 text-base-content/40" />
            <input
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              type="search"
              placeholder="Search tasks..."
              className="pl-8 pr-3 py-1.5 text-sm border border-base-300 rounded-lg bg-base-200 text-base-content focus:ring-2 focus:ring-primary focus:border-primary focus:outline-none w-52"
            />
          </div>

          {tags.length > 0 && (
            <div className="flex items-center gap-1.5 flex-wrap">
              {tags.map((tag) => (
                <button
                  key={tag.id}
                  onClick={() => toggleTag(tag.id)}
                  className={`px-2.5 py-1 text-xs rounded-full border transition-colors ${
                    selectedTagIds.includes(tag.id)
                      ? 'bg-primary text-primary-content border-primary'
                      : 'bg-base-100 text-base-content/70 border-base-300 hover:border-primary/70 hover:text-primary'
                  }`}
                >
                  {tag.name}
                </button>
              ))}
            </div>
          )}

          {hasFilters && (
            <button
              onClick={() => { setSearchTerm(''); setSelectedTagIds([]); }}
              className="text-xs text-base-content/60 hover:text-base-content flex items-center gap-1 px-2 py-1 rounded hover:bg-base-200 transition-colors"
            >
              <X className="w-3 h-3" /> Clear
            </button>
          )}
        </div>
      </header>

      {/* Board */}
      <div className="flex-1 overflow-x-auto p-6">
        {loading ? (
          <div className="flex justify-center items-center h-full">
            <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-primary" />
          </div>
        ) : (
          <DndContext
            sensors={sensors}
            collisionDetection={closestCorners}
            onDragStart={handleDragStart}
            onDragEnd={handleDragEnd}
          >
            <div className="flex gap-5 h-full items-start">
              {COLUMNS.map((col) => (
                <KanbanColumn
                  key={col.id}
                  id={col.id}
                  title={col.title}
                  tasks={tasksByStatus[col.id]}
                  onTaskClick={handleTaskClick}
                  onAddTask={handleAddTask}
                />
              ))}
            </div>
            <DragOverlay>
              {activeTask && <TaskCardPreview task={activeTask} />}
            </DragOverlay>
          </DndContext>
        )}
      </div>

      {/* Modal */}
      {isFormOpen && (
        <div
          className="fixed inset-0 bg-black/40 backdrop-blur-sm z-50 flex items-center justify-center p-4"
          onClick={(e) => { if (e.target === e.currentTarget) setIsFormOpen(false); }}
        >
          <div className="w-full max-w-lg max-h-[90vh] overflow-y-auto">
            <TaskForm
              initialData={editingTask ?? { status: defaultStatus, projectId: selectedProjectId ?? undefined }}
              isEditing={!!editingTask}
              onSave={handleSave}
              onDelete={handleDelete}
              onCancel={() => setIsFormOpen(false)}
            />
          </div>
        </div>
      )}
    </div>
  );
}
