export default function Footer() {
  return (
    <footer className="border-top py-4 mt-5">
      <div className="container text-center">
        © {new Date().getFullYear()} MiniStore
      </div>
    </footer>
  );
}
