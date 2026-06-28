import { fetchMock } from '@/services/mockService';
import type {
  Court,
  CourtComplex,
  CourtFormData,
  CourtComplexFormData,
  PagedComplexResult,
  ComplexStats,
  ManagerUser,
  CourtBookingRecord,
  CourtStatus,
} from '@/types/court.types';

const USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true';

// ─────────────────────────────────────────────
// Response helpers — backend trả về ApiResponse<T>
// ─────────────────────────────────────────────
type ApiBody<T> = { data: T };

async function getAxios() {
  const { default: axiosInstance } = await import('./axiosInstance');
  return axiosInstance;
}

/** Map CourtDto từ backend → Court type của frontend */
type RawCourtDto = {
  courtId: number;
  courtName: string;
  courtCode: string;
  courtTypeId: number;
  courtTypeName?: string;
  complexId?: number;
  complexName?: string;
  description?: string;
  location?: string;
  capacity?: number;
  surface?: string;
  imageUrl?: string;
  status: string;
  openTime: string;
  closeTime: string;
  pricePerHour: number;
  courtSize?: string;
  imageUrls?: string[];
  createdAt: string;
  updatedAt?: string;
};

function mapCourt(raw: RawCourtDto): Court {
  return {
    courtId: raw.courtId,
    courtName: raw.courtName,
    courtCode: raw.courtCode,
    courtTypeId: raw.courtTypeId,
    courtType: raw.courtTypeName
      ? { courtTypeId: raw.courtTypeId, typeName: raw.courtTypeName, isActive: true }
      : undefined,
    complexId: raw.complexId,
    description: raw.description ?? '',
    location: raw.location ?? '',
    capacity: raw.capacity,
    surface: raw.surface,
    imageUrl: raw.imageUrl ?? '',
    status: raw.status as CourtStatus,
    openTime: raw.openTime,
    closeTime: raw.closeTime,
    pricePerHour: raw.pricePerHour,
    rating: 0,
    reviewCount: 0,
    courtSize: raw.courtSize,
    imageUrls: raw.imageUrls ?? [],
    createdAt: raw.createdAt,
    updatedAt: raw.updatedAt,
  };
}

type RawUserDto = {
  userId: number;
  fullName: string;
  email: string;
  phone?: string;
  avatarUrl?: string;
  role: string;
  isActive: boolean;
};

function mapManager(raw: RawUserDto): ManagerUser {
  return {
    userId: raw.userId,
    fullName: raw.fullName,
    email: raw.email,
    phone: raw.phone,
    avatarUrl: raw.avatarUrl,
    role: 'Manager',
    isActive: raw.isActive,
  };
}

// ─────────────────────────────────────────────
// Court Complexes (Tổ hợp sân)
// ─────────────────────────────────────────────

export interface GetComplexesParams {
  search?: string;
  courtTypeId?: number;
  page?: number;
  pageSize?: number;
}

export async function getComplexes(
  params: GetComplexesParams = {}
): Promise<PagedComplexResult> {
  if (USE_MOCK) {
    const res = await fetchMock<CourtComplex[]>(() => import('@/mocks/complexes.json'));
    const all = res.data ?? [];
    return { items: all, totalCount: all.length, page: 1, pageSize: all.length, totalPages: 1 };
  }

  const axios = await getAxios();
  const response = await axios.get<ApiBody<PagedComplexResult>>('/complexes', { params });
  const result = response.data.data;

  // Backend cũ chưa trả courtTypeIds — derive từ danh sách sân
  const needsEnrich = result.items.some((cx) => !cx.courtTypeIds?.length);
  if (needsEnrich) {
    const courtsRes = await axios.get<ApiBody<RawCourtDto[]>>('/courts');
    const typeMap = new Map<number, number[]>();
    for (const c of courtsRes.data.data ?? []) {
      if (!c.complexId) continue;
      const list = typeMap.get(c.complexId) ?? [];
      if (!list.includes(c.courtTypeId)) list.push(c.courtTypeId);
      typeMap.set(c.complexId, list);
    }
    result.items = result.items.map((cx) => ({
      ...cx,
      courtTypeIds: cx.courtTypeIds?.length ? cx.courtTypeIds : (typeMap.get(cx.complexId) ?? []),
    }));
  }

  return result;
}

export async function getComplexById(complexId: number): Promise<CourtComplex | null> {
  if (USE_MOCK) {
    const res = await fetchMock<CourtComplex[]>(() => import('@/mocks/complexes.json'));
    return res.data?.find((c) => c.complexId === complexId) ?? null;
  }

  const axios = await getAxios();
  try {
    const response = await axios.get<ApiBody<CourtComplex>>(`/complexes/${complexId}`);
    return response.data.data;
  } catch {
    return null;
  }
}

export async function getComplexStats(): Promise<ComplexStats> {
  if (USE_MOCK) {
    return { totalComplexes: 2, totalCourts: 5, activeCourts: 4, maintenanceCourts: 1, inactiveCourts: 0 };
  }

  const axios = await getAxios();
  const response = await axios.get<ApiBody<ComplexStats>>('/complexes/stats');
  return response.data.data;
}

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

  const axios = await getAxios();
  const response = await axios.post<ApiBody<CourtComplex>>('/complexes', data);
  return response.data.data;
}

export async function updateComplex(
  complexId: number,
  data: Partial<CourtComplexFormData>
): Promise<CourtComplex> {
  if (USE_MOCK) {
    return { complexId, complexName: '', address: '', ...data };
  }

  const axios = await getAxios();
  const response = await axios.put<ApiBody<CourtComplex>>(`/complexes/${complexId}`, data);
  return response.data.data;
}

export async function deleteComplex(complexId: number): Promise<void> {
  if (USE_MOCK) return;
  const axios = await getAxios();
  await axios.delete(`/complexes/${complexId}`);
}

// ─────────────────────────────────────────────
// Courts (Sân thể thao)
// ─────────────────────────────────────────────

export async function getCourts(params?: {
  courtTypeId?: number;
  complexId?: number;
  status?: string;
}): Promise<Court[]> {
  if (USE_MOCK) {
    const res = await fetchMock<Court[]>(() => import('@/mocks/courts.json'));
    let courts = res.data ?? [];
    if (params?.courtTypeId) courts = courts.filter((c) => c.courtTypeId === params.courtTypeId);
    if (params?.complexId) courts = courts.filter((c) => c.complexId === params.complexId);
    if (params?.status) courts = courts.filter((c) => c.status === params.status);
    return courts;
  }

  const axios = await getAxios();
  const response = await axios.get<ApiBody<RawCourtDto[]>>('/courts', { params });
  return (response.data.data ?? []).map(mapCourt);
}

export async function getCourtById(courtId: number): Promise<Court | null> {
  if (USE_MOCK) {
    const res = await fetchMock<Court[]>(() => import('@/mocks/courts.json'));
    return res.data?.find((c) => c.courtId === courtId) ?? null;
  }

  const axios = await getAxios();
  try {
    const response = await axios.get<ApiBody<RawCourtDto>>(`/courts/${courtId}`);
    return mapCourt(response.data.data);
  } catch {
    return null;
  }
}

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

  const axios = await getAxios();
  const response = await axios.post<ApiBody<RawCourtDto>>('/courts', data);
  return mapCourt(response.data.data);
}

export async function updateCourt(
  courtId: number,
  data: Partial<CourtFormData>
): Promise<Court> {
  if (USE_MOCK) {
    return {
      courtId, courtName: '', courtCode: '', courtTypeId: 0,
      description: '', location: '', imageUrl: '', status: 'Available',
      openTime: '06:00', closeTime: '22:00', pricePerHour: 0,
      rating: 0, reviewCount: 0, createdAt: '', ...data,
    };
  }

  const axios = await getAxios();
  const response = await axios.put<ApiBody<RawCourtDto>>(`/courts/${courtId}`, data);
  return mapCourt(response.data.data);
}

export async function deleteCourt(courtId: number): Promise<void> {
  if (USE_MOCK) return;
  const axios = await getAxios();
  await axios.delete(`/courts/${courtId}`);
}

export async function getCourtTypes() {
  if (USE_MOCK) {
    return [
      { courtTypeId: 1, typeName: 'Cầu lông',   isActive: true },
      { courtTypeId: 2, typeName: 'Bóng đá',    isActive: true },
      { courtTypeId: 3, typeName: 'Pickleball',  isActive: true },
      { courtTypeId: 4, typeName: 'Tennis',      isActive: true },
      { courtTypeId: 5, typeName: 'Bóng rổ',    isActive: true },
    ];
  }

  const axios = await getAxios();
  const response = await axios.get<ApiBody<{ courtTypeId: number; typeName: string; isActive: boolean }[]>>('/court-types');
  return response.data.data;
}

// ─────────────────────────────────────────────
// Manager Users
// ─────────────────────────────────────────────

type RawUser = {
  userId: number; fullName: string; email: string;
  phone?: string; avatarUrl?: string; role: string; isActive: boolean;
};

export async function getManagerById(managerId: number): Promise<ManagerUser | null> {
  if (USE_MOCK) {
    const res = await fetchMock<RawUser[]>(() => import('@/mocks/users.json'));
    const u = (res.data ?? []).find((u) => u.userId === managerId && u.role === 'Manager');
    if (!u) return null;
    return mapManager(u);
  }

  const axios = await getAxios();
  try {
    const response = await axios.get<ApiBody<RawUserDto>>(`/users/${managerId}`);
    return mapManager(response.data.data);
  } catch {
    return null;
  }
}

export async function getManagersList(): Promise<ManagerUser[]> {
  if (USE_MOCK) {
    const res = await fetchMock<RawUser[]>(() => import('@/mocks/users.json'));
    return (res.data ?? [])
      .filter((u) => u.role === 'Manager')
      .map(mapManager);
  }

  const axios = await getAxios();
  const response = await axios.get<ApiBody<RawUserDto[]>>('/users', { params: { role: 'Manager' } });
  return (response.data.data ?? []).map(mapManager);
}

// ─────────────────────────────────────────────
// Booking History
// ─────────────────────────────────────────────

export async function getBookingsByComplexId(
  complexId: number,
  params?: { courtId?: number; status?: string; dateFrom?: string; dateTo?: string }
): Promise<CourtBookingRecord[]> {
  if (USE_MOCK) {
    const courtsRes = await fetchMock<{ courtId: number; complexId?: number }[]>(
      () => import('@/mocks/courts.json')
    );
    const courtIds = new Set(
      (courtsRes.data ?? []).filter((c) => c.complexId === complexId).map((c) => c.courtId)
    );
    const bookingsRes = await fetchMock<CourtBookingRecord[]>(() => import('@/mocks/bookings.json'));
    let list = (bookingsRes.data ?? []).filter((b) => courtIds.has(b.courtId));
    if (params?.courtId) list = list.filter((b) => b.courtId === params.courtId);
    if (params?.status)  list = list.filter((b) => b.status === params.status);
    if (params?.dateFrom) list = list.filter((b) => b.bookingDate >= params.dateFrom!);
    if (params?.dateTo)   list = list.filter((b) => b.bookingDate <= params.dateTo!);
    return list.sort((a, b) => b.bookingDate.localeCompare(a.bookingDate));
  }

  const axios = await getAxios();
  const response = await axios.get<ApiBody<CourtBookingRecord[]>>(
    `/complexes/${complexId}/bookings`,
    { params }
  );
  return response.data.data ?? [];
}
