import { fetchMock } from '@/services/mockService';
import type {
  Court,
  CourtComplex,
  CourtFormData,
  CourtComplexFormData,
  PagedComplexResult,
  ComplexStats,
} from '@/types/court.types';

const USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true';

// ─────────────────────────────────────────────
// Court Complexes (Tổ hợp sân) — unified API
// ─────────────────────────────────────────────

export interface GetComplexesParams {
  search?: string;
  courtTypeId?: number;
  page?: number;
  pageSize?: number;
}

/** Lấy danh sách tổ hợp sân (tích hợp search + filter + phân trang) */
export async function getComplexes(
  params: GetComplexesParams = {}
): Promise<PagedComplexResult> {
  if (USE_MOCK) {
    const res = await fetchMock<CourtComplex[]>(
      () => import('@/mocks/complexes.json')
    );
    const all = res.data ?? [];
    return {
      items: all,
      totalCount: all.length,
      page: 1,
      pageSize: all.length,
      totalPages: 1,
    };
  }

  const { default: axiosInstance } = await import('./axiosInstance');
  const response = await axiosInstance.get<{ data: PagedComplexResult }>(
    '/complexes',
    { params }
  );
  return response.data.data;
}

/** Lấy thống kê tổng quan hệ thống (số tổ hợp, tổng sân, đang hoạt động...) */
export async function getComplexStats(): Promise<ComplexStats> {
  if (USE_MOCK) {
    return {
      totalComplexes: 2,
      totalCourts: 5,
      activeCourts: 4,
      maintenanceCourts: 1,
      inactiveCourts: 0,
    };
  }
  const { default: axiosInstance } = await import('./axiosInstance');
  const response = await axiosInstance.get<{ data: ComplexStats }>('/complexes/stats');
  return response.data.data;
}

/** Tạo tổ hợp sân mới */
export async function createComplex(data: CourtComplexFormData): Promise<CourtComplex> {
  if (USE_MOCK) {
    return {
      complexId: Date.now(),
      ...data,
      totalCourts: 0,
      activeCourts: 0,
      createdAt: new Date().toISOString(),
    };
  }
  const { default: axiosInstance } = await import('./axiosInstance');
  const response = await axiosInstance.post<{ data: CourtComplex }>('/complexes', data);
  return response.data.data;
}

/** Cập nhật thông tin tổ hợp sân */
export async function updateComplex(
  complexId: number,
  data: Partial<CourtComplexFormData>
): Promise<CourtComplex> {
  if (USE_MOCK) {
    return { complexId, complexName: '', address: '', ...data };
  }
  const { default: axiosInstance } = await import('./axiosInstance');
  const response = await axiosInstance.put<{ data: CourtComplex }>(`/complexes/${complexId}`, data);
  return response.data.data;
}

/** Xóa tổ hợp sân */
export async function deleteComplex(complexId: number): Promise<void> {
  if (USE_MOCK) return;
  const { default: axiosInstance } = await import('./axiosInstance');
  await axiosInstance.delete(`/complexes/${complexId}`);
}

// ─────────────────────────────────────────────
// Courts (Sân thể thao)
// ─────────────────────────────────────────────

/** Lấy danh sách sân (tùy chọn lọc theo loại hoặc tổ hợp) */
export async function getCourts(params?: {
  courtTypeId?: number;
  complexId?: number;
  status?: string;
}): Promise<Court[]> {
  if (USE_MOCK) {
    const res = await fetchMock<Court[]>(
      () => import('@/mocks/courts.json')
    );
    let courts = res.data ?? [];
    if (params?.courtTypeId) {
      courts = courts.filter((c) => c.courtTypeId === params.courtTypeId);
    }
    if (params?.complexId) {
      courts = courts.filter((c) => c.complexId === params.complexId);
    }
    if (params?.status) {
      courts = courts.filter((c) => c.status === params.status);
    }
    return courts;
  }
  const { default: axiosInstance } = await import('./axiosInstance');
  const response = await axiosInstance.get<{ data: Court[] }>('/courts', { params });
  return response.data.data;
}

/** Lấy chi tiết một sân theo ID */
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

/** Tạo sân mới */
export async function createCourt(data: CourtFormData): Promise<Court> {
  if (USE_MOCK) {
    return {
      courtId: Date.now(),
      ...data,
      courtType: undefined,
      rating: 0,
      reviewCount: 0,
      createdAt: new Date().toISOString(),
    };
  }
  const { default: axiosInstance } = await import('./axiosInstance');
  const response = await axiosInstance.post<{ data: Court }>('/courts', data);
  return response.data.data;
}

/** Cập nhật thông tin sân */
export async function updateCourt(
  courtId: number,
  data: Partial<CourtFormData>
): Promise<Court> {
  if (USE_MOCK) {
    return { courtId, courtName: '', courtCode: '', courtTypeId: 0, description: '', location: '', imageUrl: '', status: 'Available', openTime: '06:00', closeTime: '22:00', pricePerHour: 0, rating: 0, reviewCount: 0, createdAt: '', ...data };
  }
  const { default: axiosInstance } = await import('./axiosInstance');
  const response = await axiosInstance.put<{ data: Court }>(`/courts/${courtId}`, data);
  return response.data.data;
}

/** Xóa sân */
export async function deleteCourt(courtId: number): Promise<void> {
  if (USE_MOCK) return;
  const { default: axiosInstance } = await import('./axiosInstance');
  await axiosInstance.delete(`/courts/${courtId}`);
}

/** Lấy danh sách loại sân */
export async function getCourtTypes() {
  if (USE_MOCK) {
    return [
      { courtTypeId: 1, typeName: 'Cầu lông',  isActive: true },
      { courtTypeId: 2, typeName: 'Bóng đá',   isActive: true },
      { courtTypeId: 3, typeName: 'Pickleball', isActive: true },
      { courtTypeId: 4, typeName: 'Tennis',     isActive: true },
      { courtTypeId: 5, typeName: 'Bóng rổ',   isActive: true },
    ];
  }
  const { default: axiosInstance } = await import('./axiosInstance');
  const response = await axiosInstance.get<{ data: { courtTypeId: number; typeName: string; isActive: boolean }[] }>('/court-types');
  return response.data.data;
}
