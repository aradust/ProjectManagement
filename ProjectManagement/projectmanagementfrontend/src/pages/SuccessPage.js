import { useEffect, useRef } from "react";
import { Button, Container, Typography, Box } from "@mui/material";
import { useNavigate } from "react-router-dom";

const useConfetti = () => {
    const canvasRef = useRef(null);

    useEffect(() => {
        const canvas = canvasRef.current;
        const ctx = canvas.getContext("2d");
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;

        const particles = [];
        const colors = ['#f44336', '#e91e63', '#9c27b0', '#673ab7', '#3f51b5', '#2196f3', '#03a9f4', '#00bcd4', '#009688', '#4caf50', '#8bc34a', '#cddc39', '#ffeb3b', '#ffc107', '#ff9800', '#ff5722'];

        for (let i = 0; i < 150; i++) {
            particles.push({
                x: canvas.width / 2,
                y: canvas.height / 2 + 100,
                w: Math.random() * 10 + 5,
                h: Math.random() * 10 + 5,
                vx: (Math.random() - 0.5) * 20,
                vy: (Math.random() - 1) * 20 - 5,
                color: colors[Math.floor(Math.random() * colors.length)],
                angle: Math.random() * 360,
                spin: (Math.random() - 0.5) * 0.2
            });
        }

        let animationId;

        const update = () => {
            ctx.clearRect(0, 0, canvas.width, canvas.height);

            let activeParticles = 0;

            particles.forEach(p => {
                p.x += p.vx;
                p.y += p.vy;
                p.vy += 0.5;
                p.angle += p.spin;
                p.vx *= 0.96;
                p.vy *= 0.96;

                if (p.y < canvas.height) activeParticles++;

                ctx.save();
                ctx.translate(p.x, p.y);
                ctx.rotate(p.angle);
                ctx.fillStyle = p.color;
                ctx.fillRect(-p.w / 2, -p.h / 2, p.w, p.h);
                ctx.restore();
            });

            if (activeParticles > 0) {
                animationId = requestAnimationFrame(update);
            }
        };

        update();

        const handleResize = () => {
            canvas.width = window.innerWidth;
            canvas.height = window.innerHeight;
        };

        window.addEventListener("resize", handleResize);

        return () => {
            cancelAnimationFrame(animationId);
            window.removeEventListener("resize", handleResize);
        };
    }, []);

    return canvasRef;
};

export default function SuccessPage() {
    const navigate = useNavigate();
    const canvasRef = useConfetti();

    return (
        <>
            <canvas
                ref={canvasRef}
                style={{
                    position: 'fixed',
                    top: 0,
                    left: 0,
                    width: '100%',
                    height: '100%',
                    zIndex: -1,
                    pointerEvents: 'none'
                }}
            />

            <Container maxWidth="sm" sx={{ height: '100vh', display: 'flex', flexDirection: 'column', justifyContent: 'center', alignItems: 'center' }}>
                <Typography variant="h2" component="h1" gutterBottom align="center" color="primary" fontWeight="bold">
                    Hooray!
                </Typography>

                <Typography variant="h5" align="center" color="text.secondary" sx={{ mb: 0 }}>
                    Project has been successfully created.
                </Typography>

                <Box sx={{ mt: 4 }}>
                    <Button
                        variant="contained"
                        size="large"
                        onClick={() => navigate("/")}
                    >
                        Go to Home
                    </Button>
                </Box>
            </Container>
        </>
    );
}