export type WindowPresetKey = 'today' | 'tomorrow' | 'next24h' | 'next7days' | 'fromCurrentHour' | 'next1hour';

export const DEFAULT_WINDOW_PRESET: WindowPresetKey = 'today';
export const FIND_PAGE_DEFAULT_WINDOW_PRESET: WindowPresetKey = 'next1hour';

export function getWindowPreset(preset: WindowPresetKey, fromDate: Date = new Date()): { windowStart: string; windowEnd: string } {
  const toISO = (d: Date) => d.toISOString();
  const startOfDayUTC = (d: Date) => {
    const x = new Date(d);
    x.setUTCHours(0, 0, 0, 0);
    return x;
  };
  const endOfDayUTC = (d: Date) => {
    const x = new Date(d);
    x.setUTCHours(23, 59, 59, 999);
    return x;
  };
  const addDays = (d: Date, n: number) => new Date(d.getTime() + n * 24 * 60 * 60 * 1000);
  const addHours = (d: Date, n: number) => new Date(d.getTime() + n * 60 * 60 * 1000);
  const startOfCurrentHourUTC = (d: Date) => {
    const x = new Date(d);
    x.setUTCMinutes(0, 0, 0);
    return x;
  };

  switch (preset) {
    case 'today': {
      const start = startOfDayUTC(fromDate);
      const end = endOfDayUTC(fromDate);
      return { windowStart: toISO(start), windowEnd: toISO(end) };
    }
    case 'tomorrow': {
      const tomorrow = addDays(fromDate, 1);
      const start = startOfDayUTC(tomorrow);
      const end = endOfDayUTC(tomorrow);
      return { windowStart: toISO(start), windowEnd: toISO(end) };
    }
    case 'next24h': {
      const start = new Date(fromDate);
      const end = addHours(start, 24);
      return { windowStart: toISO(start), windowEnd: toISO(end) };
    }
    case 'next7days': {
      const start = startOfDayUTC(fromDate);
      const end = endOfDayUTC(addDays(start, 6));
      return { windowStart: toISO(start), windowEnd: toISO(end) };
    }
    case 'fromCurrentHour': {
      const start = startOfCurrentHourUTC(fromDate);
      const end = endOfDayUTC(fromDate);
      return { windowStart: toISO(start), windowEnd: toISO(end) };
    }
    case 'next1hour': {
      const start = new Date(fromDate);
      const end = addHours(start, 1);
      return { windowStart: toISO(start), windowEnd: toISO(end) };
    }
    default:
      return getWindowPreset('today', fromDate);
  }
}

export const DURATION_OPTIONS = [15, 30, 45, 60, 90] as const;
export type DurationMinutes = (typeof DURATION_OPTIONS)[number];

export function isDurationOption(min: number): min is DurationMinutes {
  return DURATION_OPTIONS.includes(min as DurationMinutes);
}

export function getDurationLabel(minutes: number): string {
  if (minutes === 60) return '1 hr';
  if (minutes === 90) return '1.5 hr';
  return `${minutes} min`;
}
