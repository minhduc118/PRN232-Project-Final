// Common API response types
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errors?: string[];
  statusCode: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface PaginationParams {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

// Notification type
export type NotificationType = 'BookingConfirmed' | 'BookingCancelled' | 'PaymentSuccess' | 'WaitlistNotified' | 'Promotion';

export interface Notification {
  notificationId: number;
  userId: number;
  title: string;
  message: string;
  type: NotificationType;
  isRead: boolean;
  createdAt: string;
}

// Service type
export interface Service {
  serviceId: number;
  serviceName: string;
  category: string;
  description: string;
  price: number;
  stockQty: number;
  imageUrl?: string;
  isActive: boolean;
}

// Review type
export interface Review {
  reviewId: number;
  bookingId: number;
  userId: number;
  userFullName: string;
  userAvatarUrl?: string;
  courtId: number;
  rating: number;
  comment: string;
  isVisible: boolean;
  createdAt: string;
}

// Promotion type
export type DiscountType = 'Percentage' | 'FixedAmount';

export interface Promotion {
  promotionId: number;
  promoCode: string;
  promoName: string;
  discountType: DiscountType;
  discountValue: number;
  minBookingAmount?: number;
  maxDiscountAmount?: number;
  usageLimit?: number;
  usageCount: number;
  startDate: string;
  endDate: string;
  isActive: boolean;
}
