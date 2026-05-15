import { fetchMock } from '@/services/mockService';
import type { Court } from '@/types/court.types';

const USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true';

/**
 * Fetch all courts (optionally filtered by type).
 */
export async function getCourts(courtTypeId?: number): Promise<Court[]> {
  if (USE_MOCK) {
    const res = await fetchMock<Court[]>(
      () => import('@/mocks/courts.json')
    );
    const courts = res.data ?? [];
    return courtTypeId
      ? courts.filter((c) => c.courtTypeId === courtTypeId)
      : courts;
  }
  // Real API call (when backend is ready)
  const { default: axiosInstance } = await import('./axiosInstance');
  const response = await axiosInstance.get<{ data: Court[] }>('/courts', {
    params: { courtTypeId },
  });
  return response.data.data;
}

/**
 * Fetch single court detail by ID.
 */
export async function getCourtById(courtId: number): Promise<Court | null> {
  if (USE_MOCK) {
    const res = await fetchMock<Court[]>(
      () => import('@/mocks/courts.json')
    );
    return res.data?.find((c) => c.courtId === courtId) ?? null;
  }
  const { default: axiosInstance } = await import('./axiosInstance');
  const response = await axiosInstance.get<{ data: Court }>(`/courts/${courtId}`);
  return response.data.data;
}
