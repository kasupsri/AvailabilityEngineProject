import { useState, useEffect, useMemo, useRef } from 'react';
import {
  putBusy,
  getPersons,
  getBusyInWindow,
  type BusyIntervalDto,
  type PersonResponse,
  type AttendeeBusyResponse,
} from '@/api/calendarApi';
import { DateTimeField } from '@/components/DateTimeField';
import { TimeGrid, type TimeGridScale } from '@/components/TimeGrid';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import toast from 'react-hot-toast';
import { getWindowPreset, type WindowPresetKey } from '@/lib/dateWindowPresets';

export default function CalendarEditorPage() {
  const [email, setEmail] = useState('');
  const [name, setName] = useState('');
  const [persons, setPersons] = useState<PersonResponse[]>([]);
  const [intervals, setIntervals] = useState<BusyIntervalDto[]>([
    { start: new Date().toISOString(), end: new Date(Date.now() + 60 * 60 * 1000).toISOString() },
  ]);
  const [loading, setLoading] = useState(false);
  const [calendarWindowStart, setCalendarWindowStart] = useState(() => getWindowPreset('fromCurrentHour').windowStart);
  const [calendarWindowEnd, setCalendarWindowEnd] = useState(() => getWindowPreset('fromCurrentHour').windowEnd);
  const [calendarBusyData, setCalendarBusyData] = useState<AttendeeBusyResponse[] | null>(null);
  const [calendarScale, setCalendarScale] = useState<TimeGridScale>('hours');
  const loadedEmailRef = useRef<string>('');

  useEffect(() => {
    getPersons().then(setPersons).catch(() => {});
  }, []);

  useEffect(() => {
    const trimmed = email.trim().toLowerCase();
    if (!trimmed) return;
    const match = persons.find((p) => p.email.toLowerCase() === trimmed);
    if (match) setName(match.name);
  }, [email, persons]);

  useEffect(() => {
    const trimmed = email.trim().toLowerCase();
    if (!trimmed) {
      loadedEmailRef.current = '';
      setIntervals([{ start: new Date().toISOString(), end: new Date(Date.now() + 60 * 60 * 1000).toISOString() }]);
      return;
    }
    const match = persons.find((p) => p.email.toLowerCase() === trimmed);
    if (!match) {
      loadedEmailRef.current = '';
      const now = new Date();
      setIntervals([{ start: now.toISOString(), end: new Date(now.getTime() + 60 * 60 * 1000).toISOString() }]);
      return;
    }
    if (trimmed === loadedEmailRef.current) return;
    loadedEmailRef.current = trimmed;
    const now = new Date();
    const wideStart = new Date(now.getTime() - 30 * 24 * 60 * 60 * 1000).toISOString();
    const wideEnd = new Date(now.getTime() + 90 * 24 * 60 * 60 * 1000).toISOString();
    getBusyInWindow([email.trim()], wideStart, wideEnd)
      .then((data) => {
        const user = data.find((b) => b.email.toLowerCase() === trimmed);
        const existing = user?.busy ?? [];
        if (existing.length > 0) {
          setIntervals(existing.map((b) => ({ start: b.start, end: b.end })));
        } else {
          setIntervals([{ start: now.toISOString(), end: new Date(now.getTime() + 60 * 60 * 1000).toISOString() }]);
        }
      })
      .catch(() => {
        setIntervals([{ start: now.toISOString(), end: new Date(now.getTime() + 60 * 60 * 1000).toISOString() }]);
      });
  }, [email, persons]);

  const calendarAttendees = useMemo(() => {
    const trimmed = email.trim().toLowerCase();
    if (!trimmed) return persons;
    const formUser: PersonResponse = { email: email.trim(), name: name || email.trim() };
    return [formUser, ...persons.filter((p) => p.email.toLowerCase() !== trimmed)];
  }, [email, name, persons]);

  useEffect(() => {
    if (calendarAttendees.length === 0) {
      setCalendarBusyData(null);
      return;
    }
    getBusyInWindow(
      calendarAttendees.map((a) => a.email),
      calendarWindowStart,
      calendarWindowEnd
    )
      .then(setCalendarBusyData)
      .catch(() => setCalendarBusyData(null));
  }, [calendarAttendees, calendarWindowStart, calendarWindowEnd]);

  const mergedBusyData = useMemo(() => {
    if (!calendarBusyData) return null;
    const trimmed = email.trim().toLowerCase();
    let result = calendarBusyData.map((b) =>
      b.email.toLowerCase() === trimmed ? { ...b, busy: intervals } : b
    );
    if (trimmed && !result.some((b) => b.email.toLowerCase() === trimmed)) {
      result = [{ email: email.trim(), name: name || email.trim(), busy: intervals }, ...result];
    }
    return result;
  }, [calendarBusyData, email, name, intervals]);

  const applyCalendarWindowPreset = (preset: WindowPresetKey) => {
    const { windowStart: ws, windowEnd: we } = getWindowPreset(preset);
    setCalendarWindowStart(ws);
    setCalendarWindowEnd(we);
  };

  const setCalendarWindowStartSafe = (nextStart: string) => {
    setCalendarWindowStart(nextStart);
    const startMs = new Date(nextStart).getTime();
    const endMs = new Date(calendarWindowEnd).getTime();
    if (startMs >= endMs) {
      setCalendarWindowEnd(new Date(startMs + 60 * 60 * 1000).toISOString());
    }
  };

  const setCalendarWindowEndSafe = (nextEnd: string) => {
    setCalendarWindowEnd(nextEnd);
    const startMs = new Date(calendarWindowStart).getTime();
    const endMs = new Date(nextEnd).getTime();
    if (endMs <= startMs) {
      setCalendarWindowStart(new Date(endMs - 60 * 60 * 1000).toISOString());
    }
  };

  const addInterval = () => {
    const now = new Date();
    const end = new Date(now.getTime() + 60 * 60 * 1000);
    setIntervals((prev) => [...prev, { start: now.toISOString(), end: end.toISOString() }]);
  };

  const removeInterval = (index: number) => {
    setIntervals((prev) => prev.filter((_, i) => i !== index));
  };

  const updateInterval = (index: number, field: 'start' | 'end', value: string) => {
    setIntervals((prev) => {
      const next = [...prev];
      const current = next[index];
      next[index] = { ...current, [field]: value };
      const startMs = new Date(next[index].start).getTime();
      const endMs = new Date(next[index].end).getTime();
      if (startMs >= endMs) {
        if (field === 'start') next[index].end = new Date(startMs + 60 * 60 * 1000).toISOString();
        else next[index].start = new Date(endMs - 60 * 60 * 1000).toISOString();
      }
      return next;
    });
  };

  function intervalsOverlap(list: BusyIntervalDto[]): boolean {
    if (list.length < 2) return false;
    const sorted = [...list].sort((a, b) => new Date(a.start).getTime() - new Date(b.start).getTime());
    for (let i = 0; i < sorted.length - 1; i++) {
      if (new Date(sorted[i].end).getTime() > new Date(sorted[i + 1].start).getTime()) return true;
    }
    return false;
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email.trim()) {
      toast.error('Email is required');
      return;
    }
    if (!name.trim()) {
      toast.error('Name is required');
      return;
    }
    if (intervalsOverlap(intervals)) {
      toast.error('Busy slots cannot overlap. Adjust times so each slot is separate.');
      return;
    }
    setLoading(true);
    try {
      const res = await putBusy(email.trim(), intervals, name.trim());
      setPersons((prev) => {
        const has = prev.some((p) => p.email === res.email);
        if (has) return prev.map((p) => (p.email === res.email ? { ...p, name: res.name } : p));
        return [...prev, { email: res.email, name: res.name }];
      });
      setIntervals(res.busy.map((b) => ({ start: b.start, end: b.end })));
      const attendees = calendarAttendees.map((a) => a.email);
      if (attendees.length > 0) {
        const fresh = await getBusyInWindow(attendees, calendarWindowStart, calendarWindowEnd);
        setCalendarBusyData(fresh);
      }
      toast.success(
        res.busy.length === 1
          ? 'Name and 1 busy slot saved'
          : `Name and ${res.busy.length} busy slots saved`
      );
    } catch (err) {
      toast.error(err instanceof Error ? err.message : 'Failed to save');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader className="space-y-1.5">
          <CardTitle>Set your name and busy slots</CardTitle>
          <CardDescription>Enter your email and name, then add one or more busy time ranges. You can have multiple slots per person.</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit} className="space-y-6">
            <div className="space-y-4">
              <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">Who</p>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="email">Email</Label>
                <Input
                  id="email"
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="e.g. alice@example.com"
                  list="persons-list"
                />
                {persons.length > 0 && (
                  <datalist id="persons-list">
                    {persons.map((p) => (
                      <option key={p.email} value={p.email} label={p.name} />
                    ))}
                  </datalist>
                )}
              </div>
              <div className="space-y-2">
                <Label htmlFor="name">Display name</Label>
                <Input
                  id="name"
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  placeholder="e.g. Alice Smith"
                />
              </div>
              </div>
            </div>
            <div className="space-y-4">
              <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">When</p>
            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <Label>Busy slots</Label>
                <Button type="button" variant="outline" size="sm" onClick={addInterval}>
                  Add slot
                </Button>
              </div>
              <p className="text-xs text-muted-foreground">Add multiple slots for different times. Use the form above to add or edit slots.</p>
              <div className="space-y-3">
                {intervals.map((interval, i) => (
                  <div key={i} className="flex flex-col sm:flex-row flex-wrap gap-2 items-start p-3 rounded-lg border border-border/60 bg-muted/20">
                    {intervals.length > 1 && (
                      <span className="text-xs font-medium text-muted-foreground w-full sm:w-auto">Slot {i + 1}</span>
                    )}
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4 flex-1 min-w-0 w-full sm:max-w-2xl">
                      <DateTimeField
                        id={`interval-${i}-start`}
                        label="Start"
                        value={interval.start}
                        onChange={(v) => updateInterval(i, 'start', v)}
                      />
                      <DateTimeField
                        id={`interval-${i}-end`}
                        label="End"
                        value={interval.end}
                        onChange={(v) => updateInterval(i, 'end', v)}
                      />
                    </div>
                    <Button type="button" variant="outline" size="sm" onClick={() => removeInterval(i)} className="self-start sm:self-end shrink-0">
                      Remove
                    </Button>
                  </div>
                ))}
              </div>
            </div>
            </div>
            <Button type="submit" disabled={loading}>
              {loading ? 'Saving…' : 'Save name & busy slots'}
            </Button>
          </form>
        </CardContent>
      </Card>
      <Card className="border-border/80 bg-muted/5">
        <CardHeader>
          <CardTitle>Availability overview</CardTitle>
          <CardDescription>See busy times for everyone in the window. Vertical line = now. Times in your local timezone.</CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <Label>View window</Label>
            <p className="text-xs text-muted-foreground">Times in your local timezone. Default: from current hour.</p>
                <div className="flex flex-wrap gap-2">
                  <Button type="button" variant="outline" size="sm" className="rounded-full" onClick={() => applyCalendarWindowPreset('today')}>
                    Today
                  </Button>
                  <Button type="button" variant="outline" size="sm" className="rounded-full" onClick={() => applyCalendarWindowPreset('fromCurrentHour')}>
                    From current hour
                  </Button>
                  <Button type="button" variant="outline" size="sm" className="rounded-full" onClick={() => applyCalendarWindowPreset('tomorrow')}>
                    Tomorrow
                  </Button>
                  <Button type="button" variant="outline" size="sm" className="rounded-full" onClick={() => applyCalendarWindowPreset('next24h')}>
                    Next 24h
                  </Button>
                  <Button type="button" variant="outline" size="sm" className="rounded-full" onClick={() => applyCalendarWindowPreset('next7days')}>
                    Next 7 days
                  </Button>
                </div>
              </div>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <DateTimeField
                  id="calWindowStart"
                  label="Window start"
                  value={calendarWindowStart}
                  onChange={setCalendarWindowStartSafe}
                />
                <DateTimeField
                  id="calWindowEnd"
                  label="Window end"
                  value={calendarWindowEnd}
                  onChange={setCalendarWindowEndSafe}
                />
              </div>
              <div className="space-y-2">
                <Label>Scale</Label>
                <p className="text-xs text-muted-foreground">Time axis: hours, days, weeks, months, or years.</p>
                <div className="flex flex-wrap gap-2">
                  <Button
                    type="button"
                    variant={calendarScale === 'hours' ? 'default' : 'outline'}
                    size="sm"
                    className="rounded-full"
                    onClick={() => setCalendarScale('hours')}
                  >
                    Hours
                  </Button>
                  <Button
                    type="button"
                    variant={calendarScale === 'days' ? 'default' : 'outline'}
                    size="sm"
                    className="rounded-full"
                    onClick={() => setCalendarScale('days')}
                  >
                    Days
                  </Button>
                  <Button
                    type="button"
                    variant={calendarScale === 'weeks' ? 'default' : 'outline'}
                    size="sm"
                    className="rounded-full"
                    onClick={() => setCalendarScale('weeks')}
                  >
                    Weeks
                  </Button>
                  <Button
                    type="button"
                    variant={calendarScale === 'months' ? 'default' : 'outline'}
                    size="sm"
                    className="rounded-full"
                    onClick={() => setCalendarScale('months')}
                  >
                    Months
                  </Button>
                  <Button
                    type="button"
                    variant={calendarScale === 'years' ? 'default' : 'outline'}
                    size="sm"
                    className="rounded-full"
                    onClick={() => setCalendarScale('years')}
                  >
                    Years
                  </Button>
                </div>
              </div>
          <div className="w-full min-w-0 overflow-hidden rounded-lg">
            <TimeGrid
              attendees={
                calendarAttendees.length > 0
                  ? calendarAttendees
                  : [{ email: '', name: 'Enter your email above to see attendees and busy times' }]
              }
              windowStart={calendarWindowStart}
              windowEnd={calendarWindowEnd}
              busyData={calendarAttendees.length > 0 ? mergedBusyData : null}
              scale={calendarScale}
              editableEmail={email.trim() || undefined}
              onEditableIntervalsChange={setIntervals}
            />
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
