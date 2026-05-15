import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

/**
 * Merges Tailwind CSS class names safely, resolving conflicts.
 */
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

/**
 * Formats a number as Vietnamese currency (VNĐ).
 * @example formatCurrency(100000) → "100.000đ"
 */
export function formatCurrency(amount: number): string {
  return amount.toLocaleString('vi-VN') + 'đ';
}

/**
 * Formats an ISO date string to DD/MM/YYYY.
 * @example formatDate("2026-05-15") → "15/05/2026"
 */
export function formatDate(dateStr: string): string {
  const d = new Date(dateStr);
  return d.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric' });
}

/**
 * Returns a relative time string (e.g. "2 giờ trước").
 */
export function formatRelativeTime(dateStr: string): string {
  const diff = Date.now() - new Date(dateStr).getTime();
  const mins  = Math.floor(diff / 60000);
  const hours = Math.floor(diff / 3600000);
  const days  = Math.floor(diff / 86400000);
  if (mins  < 1)   return 'Vừa xong';
  if (mins  < 60)  return `${mins} phút trước`;
  if (hours < 24)  return `${hours} giờ trước`;
  return `${days} ngày trước`;
}

/**
 * Returns Vietnamese day-of-week name.
 */
export function getDayOfWeekLabel(dayIndex: number): string {
  const labels = ['CN', 'T2', 'T3', 'T4', 'T5', 'T6', 'T7'];
  return labels[dayIndex] ?? '';
}

/**
 * Generates a composite slot key for booking matrix.
 */
export function makeSlotKey(courtId: number, slotId: number): string {
  return `${courtId}-${slotId}`;
}
