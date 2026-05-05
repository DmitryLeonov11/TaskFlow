export const TaskStatus = {
  Todo: 0,
  InProgress: 1,
  Review: 2,
  Done: 3,
} as const;
export type TaskStatusValue = (typeof TaskStatus)[keyof typeof TaskStatus];

export const STATUSES: { value: TaskStatusValue; label: string }[] = [
  { value: TaskStatus.Todo, label: 'To Do' },
  { value: TaskStatus.InProgress, label: 'In Progress' },
  { value: TaskStatus.Review, label: 'Review' },
  { value: TaskStatus.Done, label: 'Done' },
];

export const PRIORITY_COLORS: Record<number, string> = {
  0: 'text-base-content/40',
  1: 'text-info',
  2: 'text-warning',
  3: 'text-error',
};

export const PRIORITY_LABELS: Record<number, string> = {
  0: 'Low',
  1: 'Medium',
  2: 'High',
  3: 'Urgent',
};

export const PRIORITIES = [
  { value: 0, label: 'Low', color: 'text-base-content/40' },
  { value: 1, label: 'Medium', color: 'text-info' },
  { value: 2, label: 'High', color: 'text-warning' },
  { value: 3, label: 'Urgent', color: 'text-error' },
];
