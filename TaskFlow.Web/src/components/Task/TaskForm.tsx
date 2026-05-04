import { useState, useEffect } from 'react';
import { X, Trash2, Plus, Check, Flag, Calendar } from 'lucide-react';
import type { Task, CreateTaskInput, UpdateTaskInput, Subtask } from '../../types';
import { useTagStore } from '../../stores/tagStore';
import { useProjectStore } from '../../stores/projectStore';
import { useTaskStore } from '../../stores/taskStore';
import api from '../../api/client';
import dayjs from 'dayjs';

const PRIORITIES = [
  { value: 0, label: 'Low', color: 'text-base-content/40' },
  { value: 1, label: 'Medium', color: 'text-info' },
  { value: 2, label: 'High', color: 'text-warning' },
  { value: 3, label: 'Urgent', color: 'text-error' },
];

interface Props {
  initialData?: Partial<Task> & { status?: number };
  isEditing: boolean;
  onSave: (data: CreateTaskInput | UpdateTaskInput) => Promise<void>;
  onDelete: (id: string) => Promise<void>;
  onCancel: () => void;
}

export default function TaskForm({ initialData, isEditing, onSave, onDelete, onCancel }: Props) {
  const { tags } = useTagStore();
  const { projects } = useProjectStore();
  const { createTask: storeCreateTask } = useTaskStore();

  const [title, setTitle] = useState(initialData?.title ?? '');
  const [description, setDescription] = useState(initialData?.description ?? '');
  const [priority, setPriority] = useState(initialData?.priority ?? 1);
  const [status, setStatus] = useState(initialData?.status ?? 0);
  const [deadline, setDeadline] = useState(
    initialData?.deadline ? dayjs(initialData.deadline).format('YYYY-MM-DD') : ''
  );
  const [selectedTags, setSelectedTags] = useState<string[]>(
    initialData?.tags?.map((t) => t.id) ?? []
  );
  const [projectId, setProjectId] = useState<string>(initialData?.projectId ?? '');
  const [subtasks, setSubtasks] = useState<Subtask[]>(initialData?.subtasks ?? []);
  const [newSubtaskTitle, setNewSubtaskTitle] = useState('');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  const toggleTag = (id: string) =>
    setSelectedTags((prev) => (prev.includes(id) ? prev.filter((t) => t !== id) : [...prev, id]));

  const handleAddSubtask = async () => {
    if (!newSubtaskTitle.trim() || !isEditing || !initialData?.id) return;
    const created = await storeCreateTask({
      title: newSubtaskTitle.trim(),
      priority: 1,
      parentTaskId: initialData.id,
    });
    setSubtasks((prev) => [
      ...prev,
      { id: created.id, title: created.title, status: 0, priority: 1, createdAt: created.createdAt },
    ]);
    setNewSubtaskTitle('');
  };

  const handleToggleSubtask = async (subtask: Subtask) => {
    if (!isEditing) return;
    const newStatus = subtask.status === 3 ? 0 : 3;
    await api.put(`/tasks/${subtask.id}`, { status: newStatus });
    setSubtasks((prev) => prev.map((s) => (s.id === subtask.id ? { ...s, status: newStatus } : s)));
  };

  const handleDeleteSubtask = async (id: string) => {
    if (!isEditing) return;
    await api.delete(`/tasks/${id}`);
    setSubtasks((prev) => prev.filter((s) => s.id !== id));
  };

  const handleSubmit = async () => {
    if (!title.trim()) { setError('Title is required'); return; }
    setSaving(true);
    setError('');
    try {
      const payload: CreateTaskInput | UpdateTaskInput = {
        title: title.trim(),
        description: description.trim() || null,
        priority,
        status,
        deadline: deadline ? new Date(deadline).toISOString() : null,
        tagIds: selectedTags,
        ...(isEditing ? {} : { projectId: projectId || null }),
      };
      await onSave(payload);
    } catch {
      setError('Failed to save task');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="bg-base-100 rounded-2xl shadow-xl border border-base-300 p-6">
      {/* Header */}
      <div className="flex items-center justify-between mb-5">
        <h2 className="text-lg font-bold text-base-content">{isEditing ? 'Edit Task' : 'New Task'}</h2>
        <button onClick={onCancel} className="p-1.5 rounded-lg hover:bg-base-200 text-base-content/50 hover:text-base-content transition-colors">
          <X className="w-4 h-4" />
        </button>
      </div>

      <div className="space-y-4">
        {/* Title */}
        <div>
          <label className="block text-xs font-medium text-base-content/70 mb-1">Title *</label>
          <input
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            placeholder="Task title"
            className="w-full px-3 py-2 text-sm border border-base-300 rounded-lg bg-base-200 text-base-content focus:ring-2 focus:ring-primary focus:border-primary focus:outline-none"
          />
        </div>

        {/* Description */}
        <div>
          <label className="block text-xs font-medium text-base-content/70 mb-1">Description</label>
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            rows={3}
            placeholder="Optional description..."
            className="w-full px-3 py-2 text-sm border border-base-300 rounded-lg bg-base-200 text-base-content focus:ring-2 focus:ring-primary focus:border-primary focus:outline-none resize-none"
          />
        </div>

        {/* Priority + Status */}
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-xs font-medium text-base-content/70 mb-1">Priority</label>
            <select
              value={priority}
              onChange={(e) => setPriority(Number(e.target.value))}
              className="w-full px-3 py-2 text-sm border border-base-300 rounded-lg bg-base-200 text-base-content focus:ring-2 focus:ring-primary focus:outline-none"
            >
              {PRIORITIES.map((p) => (
                <option key={p.value} value={p.value}>{p.label}</option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-xs font-medium text-base-content/70 mb-1">Status</label>
            <select
              value={status}
              onChange={(e) => setStatus(Number(e.target.value))}
              className="w-full px-3 py-2 text-sm border border-base-300 rounded-lg bg-base-200 text-base-content focus:ring-2 focus:ring-primary focus:outline-none"
            >
              <option value={0}>To Do</option>
              <option value={1}>In Progress</option>
              <option value={2}>Review</option>
              <option value={3}>Done</option>
            </select>
          </div>
        </div>

        {/* Deadline */}
        <div>
          <label className="block text-xs font-medium text-base-content/70 mb-1">Deadline</label>
          <input
            type="date"
            value={deadline}
            onChange={(e) => setDeadline(e.target.value)}
            className="w-full px-3 py-2 text-sm border border-base-300 rounded-lg bg-base-200 text-base-content focus:ring-2 focus:ring-primary focus:outline-none"
          />
        </div>

        {/* Project (only on create) */}
        {!isEditing && projects.length > 0 && (
          <div>
            <label className="block text-xs font-medium text-base-content/70 mb-1">Project</label>
            <select
              value={projectId}
              onChange={(e) => setProjectId(e.target.value)}
              className="w-full px-3 py-2 text-sm border border-base-300 rounded-lg bg-base-200 text-base-content focus:ring-2 focus:ring-primary focus:outline-none"
            >
              <option value="">No project</option>
              {projects.map((p) => (
                <option key={p.id} value={p.id}>{p.name}</option>
              ))}
            </select>
          </div>
        )}

        {/* Tags */}
        {tags.length > 0 && (
          <div>
            <label className="block text-xs font-medium text-base-content/70 mb-1">Tags</label>
            <div className="flex flex-wrap gap-1.5">
              {tags.map((tag) => (
                <button
                  key={tag.id}
                  type="button"
                  onClick={() => toggleTag(tag.id)}
                  className={`px-2.5 py-1 text-xs rounded-full border transition-colors ${
                    selectedTags.includes(tag.id)
                      ? 'bg-primary text-primary-content border-primary'
                      : 'bg-base-100 text-base-content/70 border-base-300 hover:border-primary/70 hover:text-primary'
                  }`}
                >
                  {tag.name}
                </button>
              ))}
            </div>
          </div>
        )}

        {/* Subtasks (only on edit) */}
        {isEditing && (
          <div>
            <label className="block text-xs font-medium text-base-content/70 mb-1">Subtasks</label>
            <div className="space-y-1 mb-2">
              {subtasks.map((s) => (
                <div key={s.id} className="flex items-center gap-2 group">
                  <button onClick={() => handleToggleSubtask(s)} className="flex-shrink-0">
                    {s.status === 3
                      ? <Check className="w-4 h-4 text-success" />
                      : <div className="w-4 h-4 rounded border border-base-300" />
                    }
                  </button>
                  <span className={`flex-1 text-sm ${s.status === 3 ? 'line-through text-base-content/40' : 'text-base-content'}`}>
                    {s.title}
                  </span>
                  <button
                    onClick={() => handleDeleteSubtask(s.id)}
                    className="opacity-0 group-hover:opacity-100 p-0.5 hover:text-error transition-opacity"
                  >
                    <X className="w-3 h-3" />
                  </button>
                </div>
              ))}
            </div>
            <div className="flex gap-2">
              <input
                value={newSubtaskTitle}
                onChange={(e) => setNewSubtaskTitle(e.target.value)}
                onKeyDown={(e) => { if (e.key === 'Enter') handleAddSubtask(); }}
                placeholder="Add subtask..."
                className="flex-1 px-2.5 py-1.5 text-xs border border-base-300 rounded-lg bg-base-200 text-base-content focus:ring-1 focus:ring-primary focus:outline-none"
              />
              <button onClick={handleAddSubtask} className="px-2.5 py-1.5 text-xs bg-base-300 rounded-lg hover:bg-base-content/10 transition-colors">
                <Plus className="w-3.5 h-3.5" />
              </button>
            </div>
          </div>
        )}

        {error && <p className="text-xs text-error">{error}</p>}

        {/* Actions */}
        <div className="flex items-center gap-2 pt-1">
          {isEditing && initialData?.id && (
            <button
              onClick={() => onDelete(initialData.id!)}
              className="p-2 rounded-lg border border-base-300 text-error hover:bg-error/10 transition-colors"
              title="Delete task"
            >
              <Trash2 className="w-4 h-4" />
            </button>
          )}
          <button
            onClick={onCancel}
            className="flex-1 px-4 py-2 text-sm border border-base-300 rounded-lg text-base-content/70 hover:bg-base-200 transition-colors"
          >
            Cancel
          </button>
          <button
            onClick={handleSubmit}
            disabled={saving}
            className="flex-1 px-4 py-2 text-sm bg-primary text-primary-content rounded-lg hover:bg-primary/90 transition-colors disabled:opacity-50"
          >
            {saving ? 'Saving...' : isEditing ? 'Update' : 'Create'}
          </button>
        </div>
      </div>
    </div>
  );
}
