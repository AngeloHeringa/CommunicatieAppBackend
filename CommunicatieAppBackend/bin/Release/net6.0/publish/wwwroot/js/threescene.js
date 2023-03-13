import { Scene } from 'three.module.js';

function main() {
    const canvas = document.querySelector('#c');
    const renderer = new THREE.WebGLRenderer({antialias: true, canvas});
    const fov = 75;
    const aspect = 2;  // the canvas default
    const near = 0.1;
    const far = 5;
    const camera = new THREE.PerspectiveCamera(fov, aspect, near, far);
    camera.position.z=2

    const scene = new THREE.Scene();

    // Get the Path in the DOM
    const path = document.querySelector(".svgpicture")[0];
    // Store the total length of the path
    const length = path.getTotalLength();

    // Empty array to store all vertices
    const vertices = [];
    // Loop along the path
    for (let i = 0; i < length; i += 0.2) {
    // Get the coordinates of a point based on the index value
    const point = path.getPointAtLength(i);
    // Create a new vector at the coordinates
    const vector = new THREE.Vector3(point.x, -point.y, 0);
    // Randomize a little bit the point to make the heart fluffier
    vector.x += (Math.random() - 0.5) * 30;
    vector.y += (Math.random() - 0.5) * 30;
    vector.z += (Math.random() - 0.5) * 70;
    // Push the vector into the array
    vertices.push(vector);
    }
    // Create a new geometry from the vertices
    const geometry = new THREE.BufferGeometry().setFromPoints(vertices);
    // Define a pink material
    const material = new THREE.PointsMaterial( { color: 0xee5282, blending: THREE.AdditiveBlending, size: 3 } );
    // Create a Points mesh based on the geometry and material
    const particles = new THREE.Points(geometry, material);
    // Offset the particles in the scene based on the viewbox values
    particles.position.x -= 600 / 2;
    particles.position.y += 552 / 2;
    // Add the particles in to the scene
    scene.add(particles);
        
}