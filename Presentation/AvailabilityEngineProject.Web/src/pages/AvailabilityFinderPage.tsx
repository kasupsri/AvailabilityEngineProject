import { useState, useEffect } from 'react';
import {
  getAvailability,
  getPersons,
  getBusyInWindow,
  type PersonResponse,
  type AttendeeBusyResponse,
} from '@/api/calendarApi';
import { DateTimeField } from '@/components/DateTimeField';
import { TimeGrid } from '@/components/TimeGrid';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Label } from '@/components/ui/label';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Copy } from 'lucide-react';
import toast from 'react-hot-toast';
import { getWindowPreset, DURATION_OPTIONS, getDurationLabel, FIND_PAGE_DEFAULT_WINDOW_PRESET, type WindowPresetKey } from '@/lib/dateWindowPresets';

const defaultWindow = () => getWindowPreset(FIND_PAGE_DEFAULT_WINDOW_PRESET);

function formatSlotTime(iso: string): string {
  const d = new Date(iso);
  const h = d.getHours();
  const m = d.getMinutes();
  const ampm = h >= 12 ? 'PM' : 'AM';
  const h12 = h % 12 || 12;
  return `${h12}:${String(m).padStart(2, '0')} ${ampm}`;
}

const WEEKDAY = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
const MONTH = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
function formatSlotDate(iso: string): string {
  const d = new Date(iso);
  return `${WEEKDAY[d.getDay()]}, ${d.getDate()} ${MONTH[d.getMonth()]}`;
}

export default function AvailabilityFinderPage() {
  const [persons, setPersons] = useState<PersonResponse[]>([]);
  const [personsLoading, setPersonsLoading] = useState(true);
  const [selectedEmails, setSelectedEmails] = useState<Set<string>>(new Set());
  const [windowStart, setWindowStart] = useState(() => defaultWindow().windowStart);
  const [windowEnd, setWindowEnd] = useState(() => defaultWindow().windowEnd);
  const [durationMinutes, setDurationMinutes] = useState(30);
  const [loading, setLoading] = useState(false);
  const [found, setFound] = useState<boolean | null>(null);
  const [slot, setSlot] = useState<{ start: string; end: string } | null>(null);
  const [busyData, setBusyData] = useState<AttendeeBusyResponse[] | null>(null);

  const applyWindowPreset = (preset: WindowPresetKey) => {
    const { windowStart: ws, windowEnd: we } = getWindowPreset(preset);
    setWindowStart(ws);
    setWindowEnd(we);
  };

  const setWindowStartSafe = (nextStart: string) => {
    setWindowStart(nextStart);
    const startMs = new Date(nextStart).getTime();
    const endMs = new Date(windowEnd).getTime();
    if (startMs >= endMs) {
      setWindowEnd(new Date(startMs + 60 * 60 * 1000).toISOString());
    }
  };

  const setWindowEndSafe = (nextEnd: string) => {
    setWindowEnd(nextEnd);
    const startMs = new Date(windowStart).getTime();
    const endMs = new Date(nextEnd).getTime();
    if (endMs <= startMs) {
      setWindowStart(new Date(endMs - 60 * 60 * 1000).toISOString());
    }
  };

  useEffect(() => {
    getPersons()
      .then((list) => {
        setPersons(list);
      })
      .catch(() => {})
      .finally(() => setPersonsLoading(false));
  }, []);

  const toggleAttendee = (email: string) => {
    setSelectedEmails((prev) => {
      const next = new Set(prev);
      if (next.has(email)) next.delete(email);
      else next.add(email);
      return next;
    });
  };

  const selectAll = () => setSelectedEmails(new Set(persons.map((p) => p.email)));
  const clearAll = () => setSelectedEmails(new Set());

  const selectedList = persons.filter((p) => selectedEmails.has(p.email));
  const selectedEmailsArray = selectedList.map((p) => p.email);

  const handleFind = async (e: React.FormEvent) => {
    e.preventDefault();
    if (selectedEmailsArray.length === 0) {
      toast.error('Select at least one attendee');
      return;
    }
    setLoading(true);
    setFound(null);
    setSlot(null);
    setBusyData(null);
    const busyPromise = getBusyInWindow(selectedEmailsArray, windowStart, windowEnd);
    try {
      const res = await getAvailability(selectedEmailsArray, windowStart, windowEnd, durationMinutes);
      setFound(res.found);
      if (res.found && res.start && res.end) setSlot({ start: res.start, end: res.end });
      toast.success(res.found ? 'Slot found' : 'No slot found');
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Request failed');
    } finally {
      try {
        const busy = await busyPromise;
        setBusyData(busy);
      } catch {
        setBusyData(null);
      }
      setLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader className="space-y-1.5">
          <CardTitle>Find a meeting slot</CardTitle>
          <CardDescription>
            Pick attendees, when to look, and meeting length. We'll show the earliest free slot.
          </CardDescription>
        </CardHeader>
        <CardContent>
          {personsLoading ? (
            <div className="space-y-2 py-2" aria-busy="true" aria-label="Loading attendees">
              <Skeleton className="h-4 w-3/4 max-w-xs" />
              <Skeleton className="h-10 w-full max-w-md" />
              <Skeleton className="h-4 w-1/2 max-w-[200px]" />
              <p className="text-sm text-muted-foreground">Loading attendees…</p>
            </div>
          ) : persons.length === 0 ? (
            <p className="text-muted-foreground py-4 text-sm">No attendees yet. Add people and their busy times on Set busy, then come back to find a slot.</p>
          ) : (
            <form onSubmit={handleFind} className="space-y-6">
              <div className="space-y-4">
                <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">Who</p>
              <div className="space-y-2">
                <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
                  <Label className="shrink-0">Attendees {selectedEmailsArray.length > 0 && `(${selectedEmailsArray.length} selected)`}</Label>
                  <div className="flex gap-2">
                    <Button type="button" variant="outline" size="sm" onClick={selectAll}>
                      Select all
                    </Button>
                    <Button type="button" variant="outline" size="sm" onClick={clearAll}>
                      Clear
                    </Button>
                  </div>
                </div>
                <div className="flex flex-wrap gap-x-3 gap-y-2 rounded-lg border border-border bg-muted/20 p-3 max-h-44 overflow-y-auto min-h-[2.5rem]">
                  {persons.map((p) => (
                    <label key={p.email} className="flex items-center gap-2 cursor-pointer shrink-0">
                      <input
                        type="checkbox"
                        checked={selectedEmails.has(p.email)}
                        onChange={() => toggleAttendee(p.email)}
                        className="rounded border-input"
                      />
                      <span className="text-sm truncate max-w-[180px] sm:max-w-none" title={`${p.name} (${p.email})`}>
                        {p.name} <span className="text-muted-foreground">({p.email})</span>
                      </span>
                    </label>
                  ))}
                </div>
              </div>
              </div>
              <div className="space-y-4">
                <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">When</p>
              <div className="space-y-2">
                <Label>When to look</Label>
                <p className="text-xs text-muted-foreground">Presets or set custom range below. All times in your local timezone.</p>
                <div className="flex flex-wrap gap-2">
                  {(['next1hour', 'today', 'fromCurrentHour', 'tomorrow', 'next24h', 'next7days'] as const).map((preset) => (
                    <Button
                      key={preset}
                      type="button"
                      variant="outline"
                      size="sm"
                      className="rounded-full"
                      onClick={() => applyWindowPreset(preset)}
                    >
                      {preset === 'next1hour'
                        ? 'Next 1h'
                        : preset === 'fromCurrentHour'
                          ? 'From current hour'
                          : preset === 'next24h'
                            ? 'Next 24h'
                            : preset === 'next7days'
                              ? 'Next 7 days'
                              : preset.charAt(0).toUpperCase() + preset.slice(1)}
                    </Button>
                  ))}
                </div>
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <DateTimeField
                  id="windowStart"
                  label="Window start"
                  value={windowStart}
                  onChange={setWindowStartSafe}
                />
                <DateTimeField
                  id="windowEnd"
                  label="Window end"
                  value={windowEnd}
                  onChange={setWindowEndSafe}
                />
              </div>
              <div className="flex flex-col gap-2 sm:flex-row sm:items-end">
                <div className="space-y-2 flex-1 min-w-0">
                  <Label htmlFor="duration">Duration</Label>
                  <Select
                    value={String(DURATION_OPTIONS.includes(durationMinutes as (typeof DURATION_OPTIONS)[number]) ? durationMinutes : 30)}
                    onValueChange={(v) => setDurationMinutes(Number(v))}
                  >
                    <SelectTrigger id="duration" className="w-full sm:w-[140px]">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {DURATION_OPTIONS.map((m) => (
                        <SelectItem key={m} value={String(m)}>
                          {getDurationLabel(m)}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>
                <Button type="submit" disabled={loading || selectedEmailsArray.length === 0} className="w-full sm:w-auto shrink-0">
                  {loading ? 'Finding…' : 'Find'}
                </Button>
              </div>
              </div>
              {found === null && selectedEmailsArray.length > 0 && (
                <p className="text-xs text-muted-foreground">Click Find to see the earliest free slot.</p>
              )}
            </form>
          )}
        </CardContent>
      </Card>
      {found !== null && (
        <Card
          className={
            found
              ? 'border-emerald-500/40 dark:border-emerald-600/30 transition-colors duration-200'
              : 'transition-colors duration-200'
          }
          aria-live="polite"
          aria-atomic="true"
        >
          <CardHeader className="space-y-1.5">
            <CardTitle>{found ? 'Slot found' : 'No slot'}</CardTitle>
            <CardDescription>
              {found
                ? 'Earliest available slot in the window.'
                : 'No slot of the given duration is free for all attendees.'}
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-3">
            {found && slot ? (
              <>
                <p className="text-base sm:text-lg font-medium">
                  {formatSlotDate(slot.start)} · {formatSlotTime(slot.start)} – {formatSlotTime(slot.end)} ({getDurationLabel(durationMinutes)})
                </p>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={() => {
                    const text = `${formatSlotDate(slot.start)} · ${formatSlotTime(slot.start)} – ${formatSlotTime(slot.end)} (${getDurationLabel(durationMinutes)})`;
                    void navigator.clipboard.writeText(text).then(() => toast.success('Copied to clipboard'));
                  }}
                  aria-label="Copy slot time to clipboard"
                >
                  <Copy className="h-4 w-4 mr-2" />
                  Copy time
                </Button>
              </>
            ) : (
              <p className="text-muted-foreground">No slot found.</p>
            )}
          </CardContent>
        </Card>
      )}
      {selectedList.length > 0 && busyData && (
        <Card>
          <CardHeader className="space-y-1.5">
            <CardTitle>Calendar</CardTitle>
            <CardDescription>
              Busy blocks for selected attendees. Green = available slot, vertical line = now.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="w-full min-w-0 overflow-hidden rounded-lg">
              <TimeGrid
                attendees={selectedList}
                windowStart={windowStart}
                windowEnd={windowEnd}
                busyData={busyData}
                foundSlot={slot}
              />
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
