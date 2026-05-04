export interface Tag {
  id: string;
  name: string;
  color?: string | null;
}

export interface TaskComment {
  id: string;
  userId: string;
  content: string;
  createdAt: string;
}

export interface TaskAttachment {
  id: string;
  fileName: string;
  fileSize: number;
  uploadedAt: string;
}

export interface Subtask {
  id: string;
  title: string;
  status: number;
  priority: number;
  createdAt: string;
}

export interface Task {
  id: string;
  title: string;
  description: string | null;
  priority: number;
  status: number;
  orderIndex: number;
  deadline?: string | null;
  createdAt: string;
  updatedAt: string;
  tags: Tag[];
  comments: TaskComment[];
  attachments: TaskAttachment[];
  projectId?: string | null;
  parentTaskId?: string | null;
  subtasks: Subtask[];
}

export interface Project {
  id: string;
  name: string;
  description?: string | null;
  color?: string | null;
  createdAt: string;
  taskCount: number;
}

export interface Notification {
  id: string;
  type: number;
  message: string;
  relatedTaskId?: string | null;
  isRead: boolean;
  createdAt: string;
}

export interface User {
  id?: string;
  email: string;
  firstName?: string;
  lastName?: string;
}

export interface CreateTaskInput {
  title: string;
  description?: string | null;
  priority: number;
  status?: number;
  deadline?: string | null;
  tagIds?: string[];
  projectId?: string | null;
  parentTaskId?: string | null;
}

export interface UpdateTaskInput {
  title?: string;
  description?: string | null;
  priority?: number;
  status?: number;
  deadline?: string | null;
  tagIds?: string[];
}

export interface CreateProjectInput {
  name: string;
  description?: string | null;
  color?: string | null;
}

export interface UpdateProjectInput {
  name?: string;
  description?: string | null;
  color?: string | null;
}
