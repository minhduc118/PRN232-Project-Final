import { fetchMock } from '@/services/mockService';
import type { Booking, CreateBookingRequest } from '@/types/booking.types';

const USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true';

/**
 * Fetch bookings for the current logged-in user.
 */
export async function getMyBookings(): Promise<Booking[]> {
  if (USE_MOCK) {
    const res = await fetchMock<Booking[]>(
      () => import('@/mocks/bookings.json')
    );
    return res.data ?? [];
  }
  const { default: axiosInstance } = await import('./axiosInstance');
  const response = await axiosInstance.get<{ data: Booking[] }>('/bookings/my');
  return response.data.data;
}

/**
 * Fetch booking detail by ID.
 */
export async function getBookingById(bookingId: number): Promise<Booking | null> {
  if (USE_MOCK) {
    const res = await fetchMock<Booking[]>(
      () => import('@/mocks/bookings.json')
    );
    return res.data?.find((b) => b.bookingId === bookingId) ?? null;
  }
  const { default: axiosInstance } = await import('./axiosInstance');
  const response = await axiosInstance.get<{ data: Booking }>(`/bookings/${bookingId}`);
  return response.data.data;
}

/**
 * Create a new booking (regular or recurring).
 */
export async function createBooking(payload: CreateBookingRequest): Promise<Booking> {
  if (USE_MOCK) {
    // Simulate creating booking — return fake data
    await new Promise((r) => setTimeout(r, 600));
    const mockBooking: Booking = {
      bookingId: Math.floor(Math.random() * 9000) + 1000,
      bookingCode: `BK-${Date.now()}`,
      userId: 2,
      courtId: payload.courtId,
      courtName: 'Sân đã chọn',
      slotId: payload.slotId,
      slotName: `${payload.bookingDate}`,
      bookingDate: payload.bookingDate,
      startTime: '17:00',
      endTime: '18:00',
      subTotal: 100000,
      discountAmount: 0,
      totalAmount: 100000,
      status: 'Pending',
      note: payload.note,
      createdAt: new Date().toISOString(),
    };
    return mockBooking;
  }
  const { default: axiosInstance } = await import('./axiosInstance');
  const response = await axiosInstance.post<{ data: Booking }>('/bookings', payload);
  return response.data.data;
}

/**
 * Cancel a booking by ID.
 */
export async function cancelBooking(bookingId: number): Promise<void> {
  if (USE_MOCK) {
    await new Promise((r) => setTimeout(r, 400));
    return;
  }
  const { default: axiosInstance } = await import('./axiosInstance');
  await axiosInstance.put(`/bookings/${bookingId}/cancel`);
}
