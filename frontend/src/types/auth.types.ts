// Auth types
export type UserRole = 'Admin' | 'Staff' | 'Coach' | 'Customer';

export interface User {
  userId:              number;
  fullName:            string;
  email:               string;
  phone?:              string;
  avatarUrl?:          string;
  role:                UserRole;
  membershipTier?:     string; // "Bronze" | "Silver" | "Gold" | "Platinum"
  loyaltyPoints?:      number;
  membershipTierId?:   number;
  membershipTierName?: string;
}

export interface LoginRequest {
  email:    string;
  password: string;
}

export interface RegisterRequest {
  fullName:        string;
  email:           string;
  phone?:          string;
  password:        string;
  confirmPassword: string;
}

/** Payload for the OTP email verification step after registration. */
export interface VerifyEmailRequest {
  email: string;
  otp:   string;
}

export interface AuthResponse {
  accessToken:  string;
  refreshToken: string;
  user:         User;
}

export interface MembershipTier {
  tierId:          number;
  tierName:        'Bronze' | 'Silver' | 'Gold' | 'Platinum';
  minPoints:       number;
  discountPercent: number;
}
