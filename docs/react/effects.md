# Effects

Rules for writing, auditing, and removing `useEffect` calls.

## The Core Rule

**Effects are for synchronizing with external systems.** If there is no external system involved (browser API, WebSocket, third-party library, DOM node), you almost certainly do not need an Effect.

Before writing `useEffect`, ask:

1. *Is this responding to a user event?* Put it in the event handler.
2. *Is this derived from existing state/props?* Calculate it during render.
3. *Is this syncing with something outside React?* Then an Effect is appropriate.

## Legitimate Uses in This Project

These are the Effect categories that belong in the codebase:

| Category | Example | Why it's correct |
|----------|---------|-----------------|
| Browser event listeners | `useKeyboardShortcut`, `shortcut:new-item` listeners | Subscribing to window events |
| WebSocket connection | `useSignalR` | External system (SignalR hub) |
| Auth event subscription | `AuthContext` token refresh listener | External system (auth service) |
| Focus management | `Step1TripDetails` auto-focus, `calendar.tsx` | Browser DOM API |
| Document title | `usePageTitle` | Browser DOM API |
| Debounce timers | `useDebouncedValue`, `useFuzzySearch` | Browser timer API |
| Timeout detection | `useSubmitTimeout` | Browser timer API |

## Anti-Patterns to Avoid

### 1. Derived state in Effects

```tsx
// BAD — extra render cycle, state can drift
const [fullName, setFullName] = useState('');
useEffect(() => {
  setFullName(firstName + ' ' + lastName);
}, [firstName, lastName]);

// GOOD — calculated during render
const fullName = firstName + ' ' + lastName;
```

### 2. Effect chains (state A changes -> Effect sets state B)

```tsx
// BAD — double render on every sort change
useEffect(() => { resetPage(); }, [sortBy, sortDirection, resetPage]);

// GOOD — handle in the event that caused the sort change
const handleSort = (column: string) => {
  toggleSort(column);
  resetPage();
};
```

### 3. Event logic in Effects

```tsx
// BAD — fires on every data refetch, not just navigation
useEffect(() => {
  if (linkParams.highlight && data.length > 0 && !data.some(r => r.id === id)) {
    toast.info("The highlighted item is not on this page.");
  }
}, [linkParams.highlight, data]);

// GOOD — derive during render, show inline
const highlightMissing = linkParams.highlight && data.length > 0
  && !data.some(r => r.id === linkParams.highlight);
// In JSX: {highlightMissing && <Alert>...</Alert>}
```

### 4. Form field cascades via Effects

```tsx
// BAD — ref-tracking to detect field changes
const prevRef = useRef(watchedCategory);
useEffect(() => {
  if (prevRef.current !== watchedCategory) {
    prevRef.current = watchedCategory;
    form.setValue("subcategory", "");
  }
}, [watchedCategory, form]);

// GOOD — reset in the onChange handler
function handleCategoryChange(value: string) {
  form.setValue("category", value);
  form.setValue("subcategory", "");
}
```

### 5. POST requests or mutations in Effects

```tsx
// BAD — runs because component rendered
useEffect(() => {
  if (submitted) post('/api/register', data);
}, [submitted]);

// GOOD — runs because user clicked submit
function handleSubmit() {
  post('/api/register', data);
}
```

### 6. Notifying parent components

```tsx
// BAD — Effect chain between parent and child
useEffect(() => {
  onChange(isOn);
}, [isOn, onChange]);

// GOOD — update parent in the same event handler
function handleToggle() {
  const next = !isOn;
  setIsOn(next);
  onChange(next);
}

// BEST — make it fully controlled (parent owns the state)
function Toggle({ isOn, onChange }) {
  return <Switch checked={isOn} onCheckedChange={onChange} />;
}
```

## Writing Correct Effects

When an Effect is genuinely needed, follow these rules:

### Dependencies must match the code

Every reactive value (prop, state, component-body variable) read inside the Effect must appear in the dependency array. The ESLint `react-hooks/exhaustive-deps` rule enforces this — **never suppress it**.

```tsx
// The linter determines the correct dependencies
useEffect(() => {
  const connection = createConnection(serverUrl, roomId);
  connection.connect();
  return () => connection.disconnect();
}, [serverUrl, roomId]); // both are reactive and read inside
```

### Always clean up

If an Effect starts something, the cleanup function must stop it.

| Setup | Cleanup |
|-------|---------|
| `addEventListener()` | `removeEventListener()` |
| `connection.connect()` | `connection.disconnect()` |
| `setTimeout()` | `clearTimeout()` |
| `setInterval()` | `clearInterval()` |
| `fetch()` | Set `ignore = true` flag |

### One Effect per synchronization process

Don't combine unrelated logic in a single Effect. Split them so each has its own dependency array and lifecycle.

```tsx
// BAD — analytics and connection in one Effect
useEffect(() => {
  logVisit(roomId);
  const conn = createConnection(roomId);
  conn.connect();
  return () => conn.disconnect();
}, [roomId]);

// GOOD — separate concerns
useEffect(() => { logVisit(roomId); }, [roomId]);
useEffect(() => {
  const conn = createConnection(roomId);
  conn.connect();
  return () => conn.disconnect();
}, [roomId]);
```

### Avoid objects and functions as dependencies

Objects and functions are new references on every render. Extract primitive values or move creation inside the Effect.

```tsx
// BAD — options is a new object every render
const options = { serverUrl, roomId };
useEffect(() => {
  const conn = createConnection(options);
}, [options]); // re-runs every render

// GOOD — depend on primitives
useEffect(() => {
  const conn = createConnection({ serverUrl, roomId });
}, [serverUrl, roomId]);
```

### Use updater functions to remove state dependencies

```tsx
// BAD — messages in dependency array causes re-subscribe on every message
useEffect(() => {
  connection.on('message', msg => {
    setMessages([...messages, msg]);
  });
}, [messages]);

// GOOD — updater function removes the dependency
useEffect(() => {
  connection.on('message', msg => {
    setMessages(prev => [...prev, msg]);
  });
}, []);
```

## Resetting Component State

Use `key` instead of Effects to reset state when context changes.

```tsx
// BAD
useEffect(() => { setComment(''); }, [userId]);

// GOOD
<Profile userId={userId} key={userId} />
```

The `key` prop tells React to destroy and recreate the component with fresh state.

## Data Fetching

**Never fetch data with raw `useEffect` + `useState`.** Use TanStack Query hooks following the patterns in `src/client/src/hooks/use*.ts`. TanStack Query handles caching, deduplication, background refetching, race conditions, and error/loading states out of the box.

If you must write a fetch Effect (e.g., in a custom hook not backed by TanStack Query), always handle race conditions:

```tsx
useEffect(() => {
  let ignore = false;
  fetchData(url).then(data => {
    if (!ignore) setData(data);
  });
  return () => { ignore = true; };
}, [url]);
```
