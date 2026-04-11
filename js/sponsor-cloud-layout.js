window.sponsorCloudLayout = (() => {
  const observers = new Map();
  const classRadii = {
    "bubble-Small": 25,
    "bubble-Medium": 35,
    "bubble-Large": 50
  };

  function getRadius(el) {
    for (const [className, radius] of Object.entries(classRadii)) {
      if (el.classList.contains(className)) return radius;
    }

    const measured = el.offsetWidth / 2;
    return Number.isFinite(measured) && measured > 0 ? measured : 25;
  }

  function overlaps(x, y, radius, placed) {
    for (const node of placed) {
      const requiredDistance = radius + node.radius + 8;
      const dx = node.x - x;
      const dy = node.y - y;
      const actualDistance = Math.sqrt((dx * dx) + (dy * dy));
      if (actualDistance < requiredDistance) return true;
    }

    return false;
  }

  function fallbackPack(nodes) {
    if (nodes.length === 0) return;

    nodes.sort((a, b) => b.radius - a.radius);
    nodes[0].x = 0;
    nodes[0].y = 0;

    const placed = [nodes[0]];
    for (const node of nodes.slice(1)) {
      let ring = 1;
      let done = false;

      while (!done) {
        for (let angle = 0; angle < 2 * Math.PI; angle += Math.PI / 24) {
          const x = ring * Math.cos(angle) * 1.6;
          const y = ring * Math.sin(angle);

          if (!overlaps(x, y, node.radius, placed)) {
            node.x = x;
            node.y = y;
            placed.push(node);
            done = true;
            break;
          }
        }

        ring += 6;
      }
    }
  }

  function applyNodePositions(field, nodes) {
    const padding = 8;
    const width = field.clientWidth || 800;

    let minX = Number.POSITIVE_INFINITY;
    let minY = Number.POSITIVE_INFINITY;
    let maxX = Number.NEGATIVE_INFINITY;
    let maxY = Number.NEGATIVE_INFINITY;

    for (const node of nodes) {
      minX = Math.min(minX, node.x - node.radius);
      minY = Math.min(minY, node.y - node.radius);
      maxX = Math.max(maxX, node.x + node.radius);
      maxY = Math.max(maxY, node.y + node.radius);
    }

    const layoutWidth = maxX - minX;
    const targetLeft = Math.max(padding, (width - layoutWidth) / 2);
    const offsetX = targetLeft - minX;
    const offsetY = padding - minY;

    for (const node of nodes) {
      node.el.style.left = `${Math.round(node.x - node.radius + offsetX)}px`;
      node.el.style.top = `${Math.round(node.y - node.radius + offsetY)}px`;
    }

    const totalHeight = Math.max(120, Math.round(maxY - minY + (padding * 2)));
    field.style.height = `${totalHeight}px`;
  }

  function centerCurrentPositions(field, bubbles) {
    const padding = 8;
    const width = field.clientWidth || 800;

    let minX = Number.POSITIVE_INFINITY;
    let minY = Number.POSITIVE_INFINITY;
    let maxX = Number.NEGATIVE_INFINITY;
    let maxY = Number.NEGATIVE_INFINITY;

    const positions = bubbles.map((el) => {
      const left = parseFloat(el.style.left || "0");
      const top = parseFloat(el.style.top || "0");
      const diameter = getRadius(el) * 2;

      minX = Math.min(minX, left);
      minY = Math.min(minY, top);
      maxX = Math.max(maxX, left + diameter);
      maxY = Math.max(maxY, top + diameter);

      return { el, left, top };
    });

    const layoutWidth = maxX - minX;
    const targetLeft = Math.max(padding, (width - layoutWidth) / 2);
    const shiftX = targetLeft - minX;
    const shiftY = padding - minY;

    for (const p of positions) {
      p.el.style.left = `${Math.round(p.left + shiftX)}px`;
      p.el.style.top = `${Math.round(p.top + shiftY)}px`;
    }

    const totalHeight = Math.max(120, Math.round(maxY - minY + (padding * 2)));
    field.style.height = `${totalHeight}px`;
  }

  function doLayout(fieldId) {
    const field = document.getElementById(fieldId);
    if (!field) return;

    const bubbles = Array.from(field.querySelectorAll(".sponsor-bubble"));
    if (bubbles.length === 0) return;

    const width = field.clientWidth || 800;
    const height = Math.max(220, Math.round(width * 0.6));
    const centerX = width / 2;
    const centerY = height / 2;

    const nodes = bubbles.map((el, i) => {
      const radius = getRadius(el);
      const angle = (2 * Math.PI * i) / bubbles.length;
      return {
        el,
        radius,
        x: centerX + Math.cos(angle) * (radius * 1.5),
        y: centerY + Math.sin(angle) * (radius * 1.5)
      };
    });

    if (typeof d3 === "undefined") {
      fallbackPack(nodes);
      applyNodePositions(field, nodes);
      if (!observers.has(fieldId)) {
        const observer = new ResizeObserver(() => layout(fieldId));
        observer.observe(field);
        observers.set(fieldId, observer);
      }
      return;
    }

    const simulation = d3
      .forceSimulation(nodes)
      .force("center", d3.forceCenter(centerX, centerY))
      .force("x", d3.forceX(centerX).strength(0.03))
      .force("y", d3.forceY(centerY).strength(0.03))
      .force("collide", d3.forceCollide().radius((d) => d.radius + 8).iterations(3))
      .stop();

    for (let i = 0; i < 240; i++) simulation.tick();

    applyNodePositions(field, nodes);

    if (!observers.has(fieldId)) {
      const observer = new ResizeObserver(() => layout(fieldId));
      observer.observe(field);
      observers.set(fieldId, observer);
    }
  }

  function layout(fieldId) {
    requestAnimationFrame(() => doLayout(fieldId));
    setTimeout(() => doLayout(fieldId), 100);
  }

  return { layout };
})();
