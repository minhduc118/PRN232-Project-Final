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

export interface CourtComplex {
  complexId: number;
  complexName: string;
  address: string;
  phone?: string;
  managerName?: string;
  managerId?: number;
  description?: string;
  imageUrl?: string;
  totalCourts?: number;
  activeCourts?: number;
  maintenanceCourts?: number;
  inactiveCourts?: number;
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
  managerName: string;
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
