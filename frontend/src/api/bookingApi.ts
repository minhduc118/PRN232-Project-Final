import type { Booking, CreateBookingRequest, BookingStatus } from '@/types/booking.types';

const USE_MOCK = import.meta.env.VITE_USE_MOCK === 'true';

// Helper to initialize and retrieve mock bookings in LocalStorage
async function getLocalStorageBookings(): Promise<Booking[]> {
  const stored = localStorage.getItem('mock_bookings');
  if (stored) {
    return JSON.parse(stored);
  }
  // Load default from json
  const { default: defaultBookings } = await import('@/mocks/bookings.json');
  localStorage.setItem('mock_bookings', JSON.stringify(defaultBookings));
  return defaultBookings as unknown as Booking[];
}

function saveLocalStorageBookings(bookings: Booking[]) {
  localStorage.setItem('mock_bookings', JSON.stringify(bookings));
}

/**
 * Fetch bookings for the current logged-in user.
 */
export async function getMyBookings(): Promise<Booking[]> {
  if (USE_MOCK) {
    await new Promise((r) => setTimeout(r, 300));
    return await getLocalStorageBookings();
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
    await new Promise((r) => setTimeout(r, 300));
    const list = await getLocalStorageBookings();
    return list.find((b) => b.bookingId === bookingId) ?? null;
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
    await new Promise((r) => setTimeout(r, 600));
    const list = await getLocalStorageBookings();
    
    // Fetch court details for naming
    const { default: courts } = await import('@/mocks/courts.json');
    const court = courts.find((c) => c.courtId === payload.courtId);
    const courtName = court ? court.courtName : 'Sân thể thao';
    const pricePerHour = court ? court.pricePerHour : 100000;

    // Fetch slot info
    const { default: slots } = await import('@/mocks/time-slots.json');
    const slot = slots.find((s) => s.slotId === payload.slotId);
    const slotName = slot ? slot.slotName : `Ca ${payload.slotId}`;
    const startTime = slot ? slot.startTime : '17:00';
    const endTime = slot ? slot.endTime : '18:00';

    // Calculate details
    const calculatedSubTotal = pricePerHour;
    let discount = 0;
    if (payload.promotionCode) {
      const { default: promos } = await import('@/mocks/promotions.json');
      const promo = promos.find((p) => p.promoCode === payload.promotionCode);
      if (promo) {
        if (promo.discountType === 'Percentage') {
          discount = (calculatedSubTotal * promo.discountValue) / 100;
        } else {
          discount = promo.discountValue;
        }
      }
    }
    const finalAmount = Math.max(0, calculatedSubTotal - discount);

    const bookingId = Math.floor(Math.random() * 900000) + 100000;
    const mockBooking: Booking = {
      bookingId,
      bookingCode: `BK-${new Date().getFullYear()}${(new Date().getMonth() + 1).toString().padStart(2, '0')}${new Date().getDate().toString().padStart(2, '0')}-${bookingId}`,
      userId: 2,
      courtId: payload.courtId,
      courtName,
      slotId: payload.slotId,
      slotName,
      bookingDate: payload.bookingDate,
      startTime,
      endTime,
      subTotal: calculatedSubTotal,
      discountAmount: discount,
      totalAmount: finalAmount,
      status: 'Pending',
      note: payload.note,
      createdAt: new Date().toISOString(),
    };

    list.push(mockBooking);
    saveLocalStorageBookings(list);
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
    const list = await getLocalStorageBookings();
    const updated = list.map((b) => {
      if (b.bookingId === bookingId) {
        return { ...b, status: 'Cancelled' as BookingStatus };
      }
      return b;
    });
    saveLocalStorageBookings(updated);
    return;
  }
  const { default: axiosInstance } = await import('./axiosInstance');
  await axiosInstance.put(`/bookings/${bookingId}/cancel`);
}

/**
 * Update payment status for a booking in mock mode.
 */
export async function updateBookingPayment(
  bookingId: number,
  status: 'Success' | 'Failed',
  method: 'VNPay' | 'MoMo' | 'Cash' | 'BankTransfer',
  transactionId?: string
): Promise<Booking> {
  if (USE_MOCK) {
    await new Promise((r) => setTimeout(r, 500));
    const list = await getLocalStorageBookings();
    const bookingIndex = list.findIndex((b) => b.bookingId === bookingId);
    if (bookingIndex === -1) throw new Error('Không tìm thấy đơn đặt sân.');

    const target = list[bookingIndex];
    const updatedBooking: Booking = {
      ...target,
      status: status === 'Success' ? 'Confirmed' : 'Pending',
      payment: {
        paymentId: Math.floor(Math.random() * 90000) + 10000,
        bookingId: target.bookingId,
        amount: target.totalAmount,
        paymentMethod: method,
        status: status === 'Success' ? 'Success' : 'Failed',
        transactionId: transactionId || `TX-${Date.now()}`,
        paidAt: status === 'Success' ? new Date().toISOString() : undefined,
      },
    };

    list[bookingIndex] = updatedBooking;
    saveLocalStorageBookings(list);
    return updatedBooking;
  }
  const { default: axiosInstance } = await import('./axiosInstance');
  const response = await axiosInstance.post<{ data: Booking }>(`/payments/update`, {
    bookingId,
    status,
    method,
    transactionId,
  });
  return response.data.data;
}

/**
 * Fetch all bookings for Admin
 */
export async function getAdminBookings(params?: { date?: string; courtTypeId?: number; status?: string }): Promise<Booking[]> {
  const { default: axiosInstance } = await import('./axiosInstance');
  const response = await axiosInstance.get<{ data: Booking[] }>('/bookings/admin', { params });
  return response.data.data;
}

export async function createBookingFromAdmin(payload: any): Promise<Booking> {
  const { default: axiosInstance } = await import('./axiosInstance');
  const response = await axiosInstance.post<{ data: Booking }>('/bookings/admin', payload);
  return response.data.data;
}

export async function updateBookingStatus(bookingId: number, payload: { status: string; cancelReason?: string }): Promise<Booking> {
  const { default: axiosInstance } = await import('./axiosInstance');
  const response = await axiosInstance.put<{ data: Booking }>(`/bookings/${bookingId}/status`, payload);
  return response.data.data;
}

