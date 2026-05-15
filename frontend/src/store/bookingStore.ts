import { create } from 'zustand';
import type { SelectedSlot } from '@/types/booking.types';

interface BookingState {
  selectedSlots: SelectedSlot[];
  selectedDate: string;
  lockedSlotIds: Set<string>; // slots temporarily locked by others (fake SignalR)

  setDate: (date: string) => void;
  addSlot: (slot: SelectedSlot) => void;
  removeSlot: (courtId: number, slotId: number) => void;
  toggleSlot: (slot: SelectedSlot) => void;
  clearSlots: () => void;
  isSlotSelected: (courtId: number, slotId: number) => boolean;

  // Fake realtime locking (simulated SignalR)
  lockSlot:   (key: string) => void;
  unlockSlot: (key: string) => void;
  isSlotLocked: (courtId: number, slotId: number) => boolean;
}

/** Composite key for a slot: "courtId-slotId" */
const slotKey = (courtId: number, slotId: number) => `${courtId}-${slotId}`;

/**
 * Booking store — manages slot selection and fake locking.
 */
export const useBookingStore = create<BookingState>()((set, get) => ({
  selectedSlots: [],
  selectedDate:  new Date().toISOString().split('T')[0],
  lockedSlotIds: new Set(),

  setDate: (date) => set({ selectedDate: date, selectedSlots: [] }),

  addSlot: (slot) =>
    set((state) => ({
      selectedSlots: [...state.selectedSlots, slot],
    })),

  removeSlot: (courtId, slotId) =>
    set((state) => ({
      selectedSlots: state.selectedSlots.filter(
        (s) => !(s.courtId === courtId && s.slotId === slotId)
      ),
    })),

  toggleSlot: (slot) => {
    const { selectedSlots, addSlot, removeSlot } = get();
    const exists = selectedSlots.some(
      (s) => s.courtId === slot.courtId && s.slotId === slot.slotId
    );
    if (exists) {
      removeSlot(slot.courtId, slot.slotId);
    } else {
      addSlot(slot);
    }
  },

  clearSlots: () => set({ selectedSlots: [] }),

  isSlotSelected: (courtId, slotId) =>
    get().selectedSlots.some((s) => s.courtId === courtId && s.slotId === slotId),

  lockSlot: (key) =>
    set((state) => ({ lockedSlotIds: new Set([...state.lockedSlotIds, key]) })),

  unlockSlot: (key) =>
    set((state) => {
      const next = new Set(state.lockedSlotIds);
      next.delete(key);
      return { lockedSlotIds: next };
    }),

  isSlotLocked: (courtId, slotId) =>
    get().lockedSlotIds.has(slotKey(courtId, slotId)),
}));
