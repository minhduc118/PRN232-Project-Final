// Court types
export type CourtStatus = 'Available' | 'Booked' | 'InUse' | 'Maintenance' | 'Inactive';
export type SlotStatus = 'Available' | 'Booked' | 'InUse' | 'Maintenance' | 'Selecting' | 'Locking';

export interface CourtType {
  courtTypeId: number;
  typeName: string;
  isActive: boolean;
}

export interface Court {
  courtId: number;
  courtName: string;
  courtCode: string;
  courtTypeId: number;
  courtType?: CourtType;
  description: string;
  location: string;
  imageUrl: string;
  status: CourtStatus;
  openTime: string;
  closeTime: string;
  pricePerHour: number;
  rating: number;
  reviewCount: number;
  createdAt: string;
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
