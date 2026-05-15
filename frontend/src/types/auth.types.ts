// Auth types
export type UserRole = 'Admin' | 'Staff' | 'Coach' | 'Customer';

export interface User {
  userId: number;
  fullName: string;
  email: string;
  phone: string;
  avatarUrl?: string;
  loyaltyPoints: number;
  membershipTierId: number;
  membershipTierName?: string;
  role: UserRole;
  isActive: boolean;
  createdAt: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  phone: string;
  password: string;
  confirmPassword: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  user: User;
}

export interface MembershipTier {
  tierId: number;
  tierName: 'Bronze' | 'Silver' | 'Gold' | 'Platinum';
  minPoints: number;
  discountPercent: number;
}
