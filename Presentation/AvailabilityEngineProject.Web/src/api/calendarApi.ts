const API_BASE = import.meta.env.VITE_API_URL ?? '';

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const text = await response.text();
    throw new Error(text || `HTTP ${response.status}`);
  }
  return response.json();
}

export interface BusyIntervalDto {
  start: string;
  end: string;
}

export interface BusyPutResponse {
  email: string;
  name: string;
  busy: { start: string; end: string }[];
}

export interface PersonResponse {
  email: string;
  name: string;
}

export async function getPersons(): Promise<PersonResponse[]> {
  const response = await fetch(`${API_BASE}/api/calendars/persons`);
  return handleResponse<PersonResponse[]>(response);
}

export async function putBusy(email: string, busy: BusyIntervalDto[], name: string): Promise<BusyPutResponse> {
  const response = await fetch(`${API_BASE}/api/calendars/${encodeURIComponent(email)}/busy`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ name, busy }),
  });
  return handleResponse<BusyPutResponse>(response);
}

export interface AvailabilityResponse {
  found: boolean;
  start?: string;
  end?: string;
}

export async function getAvailability(
  attendees: string[],
  windowStart: string,
  windowEnd: string,
  durationMinutes: number
): Promise<AvailabilityResponse> {
  const params = new URLSearchParams({
    attendees: attendees.join(','),
    windowStart,
    windowEnd,
    durationMinutes: String(durationMinutes),
  });
  const response = await fetch(`${API_BASE}/api/availability?${params}`);
  return handleResponse<AvailabilityResponse>(response);
}

export interface AttendeeBusyResponse {
  email: string;
  name: string;
  busy: { start: string; end: string }[];
}

export async function getBusyInWindow(
  attendees: string[],
  windowStart: string,
  windowEnd: string
): Promise<AttendeeBusyResponse[]> {
  if (attendees.length === 0) return [];
  const params = new URLSearchParams({
    attendees: attendees.join(','),
    windowStart,
    windowEnd,
  });
  const response = await fetch(`${API_BASE}/api/calendars/busy?${params}`);
  return handleResponse<AttendeeBusyResponse[]>(response);
}
