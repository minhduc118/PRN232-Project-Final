/**
 * Mock API service — simulates HTTP responses using local JSON files.
 * Replace these functions with real Axios calls when the backend is ready.
 */
import type { ApiResponse, PagedResult } from '@/types/common.types';

/** Simulates network latency (ms) */
const FAKE_DELAY = 400;

const delay = (ms: number) => new Promise<void>((r) => setTimeout(r, ms));

/**
 * Wraps mock data in the standard ApiResponse envelope.
 */
export function mockOk<T>(data: T, message = 'Success'): ApiResponse<T> {
  return { success: true, message, data, statusCode: 200 };
}

/**
 * Returns a paged result from a full array.
 */
export function mockPaged<T>(
  items: T[],
  pageNumber = 1,
  pageSize = 10,
): ApiResponse<PagedResult<T>> {
  const totalCount = items.length;
  const totalPages = Math.ceil(totalCount / pageSize);
  const start = (pageNumber - 1) * pageSize;
  const sliced = items.slice(start, start + pageSize);

  return mockOk<PagedResult<T>>({
    items: sliced,
    totalCount,
    pageNumber,
    pageSize,
    totalPages,
    hasNextPage: pageNumber < totalPages,
    hasPreviousPage: pageNumber > 1,
  });
}

/**
 * Generic fetch wrapper with fake delay.
 * JSON imports are cast via `unknown` to avoid strict type conflicts.
 */
export async function fetchMock<T>(
  loader: () => Promise<unknown>,
): Promise<ApiResponse<T>> {
  await delay(FAKE_DELAY);
  const module = await loader();
  // JSON modules export their value as `default`
  const data = (module as { default: T }).default;
  return mockOk(data);
}
