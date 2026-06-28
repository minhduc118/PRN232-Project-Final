// Booking types
export type BookingStatus = 'Pending' | 'Confirmed' | 'Cancelled' | 'Completed' | 'NoShow';
export type PaymentMethod = 'VNPay' | 'MoMo' | 'Cash' | 'BankTransfer';
export type PaymentStatus = 'Pending' | 'Success' | 'Failed' | 'Refunded';

export interface SelectedSlot {
  courtId: number;
  courtName: string;
  slotId: number;
  slotName: string;
  startTime: string;
  endTime: string;
  date: string;
  price: number;
}

export interface Booking {
  bookingId: number;
  bookingCode: string;
  userId: number;
  customerName?: string;
  customerPhone?: string;
  courtId: number;
  courtName: string;
  slotId: number;
  slotName: string;
  bookingDate: string;
  startTime: string;
  endTime: string;
  subTotal: number;
  discountAmount: number;
  totalAmount: number;
  status: BookingStatus;
  promotionId?: number;
  promotionCode?: string;
  note?: string;
  createdAt: string;
  payment?: Payment;
}

export interface BookingService {
  serviceId: number;
  serviceName: string;
  quantity: number;
  totalPrice: number;
}

export interface CreateBookingRequest {
  courtId: number;
  slotId: number;
  bookingDate: string;
  serviceIds?: { serviceId: number; quantity: number }[];
  promotionCode?: string;
  note?: string;
  // Recurring
  isRecurring?: boolean;
  recurringDays?: number[];   // 0=Sun, 1=Mon, ..., 6=Sat
  recurringEndDate?: string;
}

export interface Payment {
  paymentId: number;
  bookingId: number;
  amount: number;
  paymentMethod: PaymentMethod;
  transactionId?: string;
  status: PaymentStatus;
  paidAt?: string;
  refundAmount?: number;
}

// Waitlist
export type WaitlistStatus = 'Waiting' | 'Notified' | 'Confirmed' | 'Expired';

export interface Waitlist {
  waitlistId: number;
  userId: number;
  courtId: number;
  courtName: string;
  slotId: number;
  slotName: string;
  waitDate: string;
  position: number;
  status: WaitlistStatus;
  notifiedAt?: string;
  expiredAt?: string;
}

// Recurring Booking
export interface RecurringBooking {
  recurringId: number;
  userId: number;
  courtId: number;
  courtName: string;
  slotId: number;
  slotName: string;
  startDate: string;
  endDate: string;
  daysOfWeek: number[];
  status: 'Active' | 'Paused' | 'Cancelled';
}
