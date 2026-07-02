// Court types
export type CourtStatus = 'Available' | 'Booked' | 'InUse' | 'Maintenance' | 'Inactive';
export type SlotStatus = 'Available' | 'Booked' | 'InUse' | 'Maintenance' | 'Selecting' | 'Locking';

export interface CourtType {
  courtTypeId: number;
  typeName: string;
  iconUrl?: string;
  description?: string;
  isActive: boolean;
}

/** User có role Manager — truy vấn qua API khi cần, không nhúng thẳng vào CourtComplex */
export interface ManagerUser {
  userId: number;
  fullName: string;
  email: string;
  phone?: string;
  avatarUrl?: string;
  role: 'Manager';
  isActive: boolean;
}

/** Booking record — dùng cho lịch sử thuê sân */
export interface CourtBookingRecord {
  bookingId: number;
  bookingCode: string;
  userId: number;
  customerName?: string;
  customerPhone?: string;
  courtId: number;
  courtName?: string;
  bookingDate: string;
  startTime: string;
  endTime: string;
  totalAmount: number;
  status: 'Pending' | 'Confirmed' | 'Cancelled' | 'Completed' | 'NoShow';
  paymentMethod?: string;
  paymentStatus?: string;
  createdAt: string;
}

export interface CourtComplex {
  complexId: number;
  complexName: string;
  address: string;
  phone?: string;
  /** Chỉ lưu mã quản lý. Khi cần thông tin đầy đủ gọi getManagerById(managerId) */
  managerId?: number;
  /** Readonly — populate từ API join, không lưu trong form */
  managerName?: string;
  description?: string;
  imageUrl?: string;
  totalCourts?: number;
  activeCourts?: number;
  maintenanceCourts?: number;
  inactiveCourts?: number;
  /** Các loại sân có trong tổ hợp — derive từ courts hoặc API trả về */
  courtTypeIds?: number[];
  createdAt?: string;
}

export interface PagedComplexResult {
  items: CourtComplex[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ComplexStats {
  totalComplexes: number;
  totalCourts: number;
  activeCourts: number;
  maintenanceCourts: number;
  inactiveCourts: number;
}

export interface Court {
  courtId: number;
  courtName: string;
  courtCode: string;
  courtTypeId: number;
  courtType?: CourtType;
  complexId?: number;
  complex?: CourtComplex;
  description: string;
  location: string;
  capacity?: number;
  surface?: string;
  imageUrl: string;
  status: CourtStatus;
  openTime: string;
  closeTime: string;
  pricePerHour: number;
  rating: number;
  reviewCount: number;
  courtSize?: string;
  imageUrls?: string[];
  createdAt: string;
  updatedAt?: string;
}

export interface CourtFormData {
  courtName: string;
  courtCode: string;
  courtTypeId: number;
  complexId?: number;
  description: string;
  location: string;
  capacity: number;
  surface: string;
  imageUrl: string;
  status: CourtStatus;
  openTime: string;
  closeTime: string;
  pricePerHour: number;
  courtSize?: string;
  imageUrls?: string[];
}

export interface CourtComplexFormData {
  complexName: string;
  address: string;
  phone: string;
  /** Chỉ truyền mã quản lý. Thông tin hiển thị fetch riêng qua getManagerById() */
  managerId?: number;
  description: string;
  imageUrl: string;
}

export interface TimeSlot {
  slotId: number;
  slotName: string;
  startTime: string;
  endTime: string;
  dayType: 'Weekday' | 'Weekend';
}

export interface CourtPricing {
  pricingId: number;
  courtId: number;
  slotId: number;
  price: number;
  peakMultiplier?: number;
  effectiveFrom: string;
}

export interface SlotAvailability {
  courtId: number;
  slotId: number;
  date: string;
  status: SlotStatus;
  lockedByUserId?: number;
  lockedUntil?: string;
}
